using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Onkyo
{
	public class Receiver
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

		public struct Song
		{
			public string title;
			public string album;
			public string artist;
			public int index;
		};

		public Song currentSong = new Song();

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

				SendCommand("PWRQSTN");
				SendCommand("ATMQSTN");
				SendCommand("MVLQSTN");
				SendCommand("SLIQSTN");
				SendCommand("NFIQSTN");
				//SendCommand("NRIQSTN");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				Task.Run(async () => await readPackets()); //run in background
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			}
			catch (IOException e)
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
					if (command.command == "PWR")
						_power = command.data == "01";
					if (command.command == "SLI")
						_input = GetInputFromPacket(command.data);
					if (command.command == "NLS") // net-usb-list-info
						;

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
				_volume = newVolume;
			}
			catch (FormatException)
			{
				Console.WriteLine("Error parsing new volume: " + command.data + "...");
			}
		}


		bool _power = false;
		public bool Power {
			get { return _power; }
			set { SendCommand("PWR0" + (value ? "1" : "0")); }
		}

		private string _input;
		public string Input {
			get { return _input; }
			set {
				if (inputMap.ContainsValue(value))
				{
					string packet = inputMap.First(i => i.Value == value).Key;
					SendCommand("SLI" + packet);
				}
			}
		}

		private int _volume;
		public int Volume {
			get { return _volume; }
			set { SendCommand("MVL" + value.ToString("X2").ToUpper()); }
		}



		public void Play() { SendCommand("NTCPLAY"); }
		public void Pause() { SendCommand("NTCPAUSE"); }
		public void Stop() { SendCommand("NTCPAUSE"); }
		public void Next() { SendCommand("NTCTRUP"); }
		public void Prev() { SendCommand("NTCTRDN"); }



		public void SendCommand(string command)
		{
			Header header = new Header(command.Length + 3);
			header.WriteToStream(stream);
			stream.Write(System.Text.Encoding.ASCII.GetBytes("!1"));
			stream.Write(System.Text.Encoding.ASCII.GetBytes(command));
			stream.WriteByte(0x0d);
		}


		private Dictionary<string, string> inputMap = new Dictionary<string, string>()
		{
			{  "01", "Wiiu" },
            {  "02", "Aux" },
            {  "03", "Aux" },
            {  "05", "PC" },
			{  "10", "Kodi" },
			{  "11", "Chromecast" },
			{  "12", "TV" },
			{  "22", "Phono" },
			{  "23", "CD" },
			{  "24", "FM" },
			{  "25", "AM" },
			{  "26", "Tuner" },
			{  "27", "Music Server, P4S, DLNA" },
			{  "28", "Internet Radio" },
			{  "2B", "Network" },
		};

		public IEnumerable<string> getInputs()
		{
			return inputMap.Values;
		}
		private string GetInputFromPacket(string packet)
		{
			if (!inputMap.ContainsKey(packet))
			{
				Console.WriteLine("Onkyo\t\tCould not find input type for " + packet);
                return "error";
			}
			return inputMap[packet];
		}

	}
}
