using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Onkyo
{
	class Header
	{
		public char[] magic = new char[4];
		public int headerSize;
		public int packetSize;
		public byte[] flags = new byte[4];

		public Header()
		{
		}

		public Header(int packetSize)
		{
			this.magic = new char[] { 'I', 'S', 'C', 'P' };
			this.flags = new byte[] { 1, 0, 0, 0 };
			this.headerSize = 16;
			this.packetSize = packetSize;
		}

		public async Task<Header> ReadFromStreamAsync(NetworkStream stream)
		{
			byte[] buffer = new byte[16];
			int count = 0;
			while (count < 16)
			{
				int rc = await stream.ReadAsync(buffer, count, 16 - count);
				if (rc == 0)
					throw new IOException("Disconnected from onkyo");
				count += rc;
			}

			for (int i = 0; i < 4; i++)
				magic[i] = (char)buffer[i];
			//eww, IPAddress.HostToNetworkOrder should be a better function
			headerSize = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buffer, 4));
			packetSize = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(buffer, 8));

			for (int i = 0; i < 4; i++)
				flags[i] = buffer[12 + i];

			Debug.Assert(magic[0] == 'I' && magic[1] == 'S' && magic[2] == 'C' && magic[3] == 'P', "Magic header for onkyo data is not valid");
			Debug.Assert(headerSize == 16, "Header should be 16 long");
			return this;
		}

		public void WriteToStream(NetworkStream stream)
		{
			stream.Write(System.Text.Encoding.ASCII.GetBytes(magic));
			stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(headerSize)));
			stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(packetSize)));
			stream.Write(flags);
		}
	}
}