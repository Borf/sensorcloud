using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Onkyo
{
	public class Command
	{
		private static dynamic commands;
		static Command()
		{
			commands = JObject.Parse(File.ReadAllText("eiscp-commands.json"));
		}


		public String command;
		public String name;
		public byte[] rawData;
		public String data;


		internal async Task<Command> ReadFromStreamAsync(NetworkStream stream, Header header)
		{
			rawData = new byte[header.packetSize];
			int bytesRead = 0;
			while (bytesRead < header.packetSize)
			{
				int rc = await stream.ReadAsync(rawData, bytesRead, header.packetSize - bytesRead);
				if (rc == 0)
					throw new IOException("Disconnected from onkyo");
				bytesRead += rc;
			}

			command = System.Text.Encoding.ASCII.GetString(rawData, 2, 3);
			data = System.Text.Encoding.UTF8.GetString(rawData, 5, rawData.Length - 5).TrimEnd(new char[] { '\r', '\n', (char)0x1a } );

			foreach (JProperty zone in commands["commands"])
				if(zone.Value[command] != null)
					this.name = zone.Value[command]["name"].ToString();

			return this;
		}


	}
}
