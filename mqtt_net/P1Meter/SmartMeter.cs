using System;
using System.IO;
using System.Diagnostics;

namespace P1Meter
{
    public class SmartMeter
    {
        string buffer;

        public event EventHandler<DataPacket> OnData;

        public async void Connect(string port)
        {
			Console.WriteLine("Starting");

			ProcessStartInfo startInfo = new ProcessStartInfo() 
			{ 
				FileName = "/bin/stty", 
				Arguments = "-F /dev/ttyUSB0 9600 cs7 -cstopb parenb -parodd -cmspar -hupcl -crtscts -ixon", 
			}; 
			Process proc = new Process() { StartInfo = startInfo, };
			proc.Start();
			proc.WaitForExit();
			
			using (StreamReader sr = new StreamReader("/dev/ttyUSB0"))
			{
				while(true)
				{
					string line = await sr.ReadLineAsync();
					if(line != null)
						DataReceived(this, line + "\n");
				}
			}
        }

        private void DataReceived(object sender, string data)
        {
        	if(data.Trim() == "")
        		return;
            buffer += data;
            if (buffer.EndsWith("!\n"))
            {
                DataPacket packet = new DataPacket();
                var lines = buffer.Split("\n");
                for (var i = 0; i < lines.Length; i++)
                {
                    if(lines[i].StartsWith("0-1:24.3"))
                    {
                        packet.ParseLine("0-1:24.2.1" + lines[i+1]);
					}
                    else
                        packet.ParseLine(lines[i]);
                }
                
                if(packet.IsValid)
                    OnData?.Invoke(this, packet);
				else
					Console.WriteLine($"Invalid packet: {packet.ToString()}");
                buffer = "";
            }
        }
    }
}
