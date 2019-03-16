using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace JvcProjector
{
	public class Projector
	{
		private string address;
		private TcpClient tcpClient;
		private NetworkStream stream;
		private bool ready = false;
		private PowerStatus _status = PowerStatus.notloaded;
		public event EventHandler<PowerStatus> StatusChange;

		public PowerStatus Status {
			get { return _status; }
			set {
				stream.Write(new byte[] { 0x21, 0x89, 0x01, 80,87, (byte)((value == PowerStatus.poweron) ? 49 : 48), 0x0A });
			}
		}

		public async Task Connect(string address)
		{
			this.address = address;

			try
			{
				ready = false;
				tcpClient = new TcpClient();
				Console.WriteLine($"JVC\t\tConnecting to {address}");
				await tcpClient.ConnectAsync(address, 20554);
				Console.WriteLine($"JVC\t\tConnected to {address}");
				stream = tcpClient.GetStream();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				Task.Run(async () => await ReadPackets()); //run in background
				Task.Run(async () => await UpdateStatus()); //run in background
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			catch (IOException e)
			{
				Console.WriteLine($"JVC\t\tException when reading from socket: {e}");
			}
			catch (SocketException e)
			{
				Console.WriteLine($"JVC\t\tException when connecting to socket: {e}");
			}
		}

		private async Task ReadPackets()
		{
			try
			{
				byte[] buffer = new byte[1024];
				int count = 0;
				while (true)
				{
					int rc = await stream.ReadAsync(buffer, count, 1024 - count);
					if (rc == 0)
						throw new IOException("Disconnected from projector");
					count += rc;

					while (count > 0)
					{
						if (count >= 5 && System.Text.Encoding.ASCII.GetString(buffer, 0, 5) == "PJ_OK")
						{
							await stream.WriteAsync(System.Text.Encoding.ASCII.GetBytes("PJREQ"));
							count -= 5;
							if (count > 0)
								Buffer.BlockCopy(buffer, 5, buffer, 0, count);
							continue;
						}
						if (count >= 5 && System.Text.Encoding.ASCII.GetString(buffer, 0, 5) == "PJACK")
						{
							//hands have been shaked
							ready = true;
							count -= 5;
							if (count > 0)
								Buffer.BlockCopy(buffer, 5, buffer, 0, count);
							continue;
						}
						if (count >= 5 && System.Text.Encoding.ASCII.GetString(buffer, 0, 5) == "PJ_NG")
						{
							//no udea
							count -= 5;
							if (count > 0)
								Buffer.BlockCopy(buffer, 5, buffer, 0, count);
							continue;
						}
						//				♠	ë    P  W  ◙  @  ë  ☺  P  W  0  ◙
						//status reply: 06 89 01 50 57 0a 40 89 01 50 57 30 0a
						if (count >= 6 &&
							buffer[0] == 0x06 &&
							buffer[1] == 0x89 &&
							buffer[2] == 0x01 &&
							buffer[3] == 0x50 &&
							buffer[4] == 0x57 &&
							buffer[5] == 0x0a
							)
						{
							count -= 6;
							if (count > 0)
								Buffer.BlockCopy(buffer, 6, buffer, 0, count);
							continue;
						}

						if (count >= 7 &&
							buffer[0] == 0x40 &&
							buffer[1] == 0x89 &&
							buffer[2] == 0x01 &&
							buffer[3] == 0x50 &&
							buffer[4] == 0x57 &&
							buffer[6] == 0x0a
							)
						{
							PowerStatus newStatus = PowerStatus.emergency;
							switch (buffer[5])
							{
								case 0x30: newStatus = PowerStatus.standby; break;
								case 0x31: newStatus = PowerStatus.poweron; break;
								case 0x32: newStatus = PowerStatus.cooling; break;
								case 0x34: newStatus = PowerStatus.emergency; break;
							}
							if (newStatus != _status)
							{
								StatusChange?.Invoke(this, newStatus);
								_status = newStatus;
							}

							count -= 7;
							if (count > 0)
								Buffer.BlockCopy(buffer, 7, buffer, 0, count);
							continue;
						}
						break;
					}
					if (count > 0)
					{
						throw new IOException("Protocol error, unknown command");
					}
				}
			}
			catch (IOException e)
			{
				Console.WriteLine($"ONKYO\tException when reading from socket: {e}");
			}
			finally
			{
				await Task.Delay(1000);
				await Connect(address);
			}
		}

		private async Task UpdateStatus()
		{
			while (true)
			{
				if (this.ready)
					this.stream.Write(new byte[] { 0x3f, 0x89, 0x01, 80, 87, 0x0A });
				await Task.Delay(5000);
			}
		}

	}
}
