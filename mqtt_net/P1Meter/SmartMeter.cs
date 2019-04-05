using System;
using System.IO;
using System.IO.Ports;

namespace P1Meter
{
    public class SmartMeter
    {
        SerialDevice serial;
        string buffer;

        public event EventHandler<DataPacket> OnData;

        public void Connect(string port)
        {
            serial = new SerialDevice(port, BaudRate.B9600);

            serial.DataReceived += DataReceived;
            serial.Open();
            System.Console.WriteLine("Connected");
        }

        private void DataReceived(object sender, byte[] data)
        {
            buffer += System.Text.Encoding.UTF8.GetString(data);
            if (buffer.EndsWith("!\n"))
            {
                DataPacket packet = new DataPacket();
                var lines = buffer.Split("\n");
                for (var i = 0; i < lines.Length; i++)
                {
                    if(lines[i].StartsWith("0-1:24.3"))
                        packet.ParseLine("0-1:24.2.1" + lines[i+2]);
                    else
                        packet.ParseLine(lines[i]);
                }
                if(packet.IsValid)
                    OnData?.Invoke(this, packet);
                buffer = "";
            }
        }
    }
}
