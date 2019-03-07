using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Onkyo
{
    public partial class Receiver
	{
		private TcpClient tcpClient;
        private NetworkStream stream;
		private string address;

		public event EventHandler<Command> Data;
		public event EventHandler<int> VolumeChange;
        public event EventHandler<Command> TrackChange;
        public event EventHandler<PlayStatus> PlayStatusChange;
        public event EventHandler<RepeatStatus> RepeatStatusChange;
        public event EventHandler<ShuffleStatus> ShuffleStatusChange;


        public int volume { get; private set; }

        struct Song
        {
            public string title;
            public string album;
            public string artist;
            public int index;
        };

        Song currentSong = new Song();

        public Status status = new Status();

        public Receiver()
		{
		}

		public async Task Connect(string address)
		{
			this.address = address;

			try
			{
				tcpClient = new TcpClient();
				Console.WriteLine($"ONKYO\tConnecting to {address}");
				await tcpClient.ConnectAsync(address, 60128);
				Console.WriteLine($"ONKYO\tConnected to {address}");
                stream = tcpClient.GetStream();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(async () => await readPackets()); //run in background
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			} catch(IOException e)
			{
				Console.WriteLine($"ONKYO\tException when reading from socket: {e}");
			}
		}

		private async Task readPackets()
		{
			try
			{
				while (true)
				{
					Header header = await new Header().ReadFromStreamAsync(stream);
					Command command = await new Command().ReadFromStreamAsync(stream, header);
					Data?.Invoke(this, command);
                    if (command.command == "MVL") // master volume
                        handleVolume(command);
                    if (command.command == "NST") //status
                        handleStatus(command);
                    if (command.command == "NAT") // artist
                        currentSong.artist = command.data;
                    if (command.command == "NAL") // album
                        currentSong.album = command.data;
                    if (command.command == "NTI") // title
                        currentSong.title = command.data;
                    if (command.command == "NTR") // index in playlist
                        currentSong.index = int.Parse(command.data.Substring(0, command.data.IndexOf("/")));

                }
            } catch(IOException e)
			{
				Console.WriteLine($"ONKYO\tException when reading from socket: {e}");
			} finally
			{
				await Task.Delay(1000);
				await Connect(address);
			}
		}

        private void handleStatus(Command command)
        {
            string data = command.data;

            Status newStatus = new Status(data);
            if (status.playStatus != newStatus.playStatus)
                PlayStatusChange?.Invoke(this, newStatus.playStatus);
            if (status.shuffleStatus != newStatus.shuffleStatus)
                ShuffleStatusChange?.Invoke(this, newStatus.shuffleStatus);
            if (status.repeatStatus != newStatus.repeatStatus)
                RepeatStatusChange?.Invoke(this, newStatus.repeatStatus);
            status = newStatus;
        }

        private void handleVolume(Command command)
        {
            try
            {
                int newVolume = Convert.ToInt32(command.data, 16);
                VolumeChange?.Invoke(this, newVolume);
                volume = newVolume;
            }
            catch (FormatException)
            {
                Console.WriteLine("Error parsing new volume: " + command.data + "...");
            }
        }

        public void SetVolume(int newVolume)
        {
            String hexValue = newVolume.ToString("X2");
            hexValue = hexValue.ToUpper();

            SendCommand("MVL" + hexValue);
        }

        public void SetPower(bool status)
        {
            SendCommand("PWR0" + (status ? "1" : "0"));
        }

        public void Play() { SendCommand("NTCPLAY"); }
        public void Pause() { SendCommand("NTCPAUSE"); }
        public void Stop() { SendCommand("NTCPAUSE"); }
        public void Next() { SendCommand("NTCTRUP"); }
        public void Prev() { SendCommand("NTCTRDN"); }



        private void SendCommand(string command)
        {
            Header header = new Header(command.Length+3);
            header.WriteToStream(stream);
            stream.Write(System.Text.Encoding.ASCII.GetBytes("!1"));
            stream.Write(System.Text.Encoding.ASCII.GetBytes(command));
            stream.WriteByte(0x0d);
        }
    }
}
