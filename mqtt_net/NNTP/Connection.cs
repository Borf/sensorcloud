using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NNTP
{
    public class Connection
    {
        private string host;
        private int port;
        private string username;
        private string password;

        private NetworkStream stream;

        TcpClient client;

        public Connection()
        {
            client = new TcpClient();
        }

        public async Task Connect(string host, int port, string username, string password)
        {
            this.host = host;
            this.port = port;
            this.username = username;
            this.password = password;

            Console.WriteLine("nntp: connecting to server");
            await client.ConnectAsync(this.host, this.port);
            //client.Connect(this.host, this.port);

            stream = client.GetStream();
            byte[] buffer = new byte[1024];
            string lineBuffer = "";
            while(client.Connected)
            {
                //int rc = await stream.ReadAsync(buffer, 0, buffer.Length);
                int rc = await stream.ReadAsync(buffer, 0, buffer.Length);

                Console.WriteLine($"Got {rc} bytes");
                lineBuffer += Encoding.GetEncoding("ISO-8859-1").GetString(buffer, 0, rc);
                if(lineBuffer.Contains("\n"))
                {
                    string line = lineBuffer.Substring(0, lineBuffer.IndexOf("\n")).Trim();
                    lineBuffer = lineBuffer.Substring(lineBuffer.IndexOf("\n") + 1);
                    var cmd = line.Split(" ");
                    if (cmd[0] == "200")
                        await Send($"AUTHINFO USER {username}\r\n");
                    else if (cmd[0] == "381") //need more
                        await Send($"AUTHINFO PASS {password}\r\n");
                    else
                    {
                        Console.WriteLine($"Unknown line: {line}");
                        break;
                    }
                }

            }
//            return Task.CompletedTask;
        }

        public void Disconnect()
        {
            client.Close();
        }

        public async Task<Group> Group(string name)
        {
            await Send($"GROUP {name}\r\n");
            string line = await readline();
            var p = line.Split(" ");
            return new Group()
            {
                number = long.Parse(p[1]),
                low = long.Parse(p[2]),
                high = long.Parse(p[3]),
            };
        }

        public async Task<Header> Headers(long articleId)
        {
            await Send($"HEAD {articleId}\r\n");
            string ack = await readline();
            string[] d = ack.Split(" ");
            if (d[0] != "221")
                return null;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            while(true)
            {
                string line = (await readline()).Trim();
                if (line == ".")
                    break;
                var h = line.Split(":", 2);
                if (h.Length != 2)
                    return null;
                string key = h[0].Trim().ToUpper();
                if (!headers.ContainsKey(key))
                    headers[key] = h[1].Trim();
                else
                    headers[key] += h[1].Trim();
            }

            return new Header()
            {
                articleNumber = articleId,
                articleId = d[2].Trim(),
                headers = headers
            };
        }

        public async Task<string> Body(string id)
        {
            await Send($"BODY {id}\r\n");
            string header = await readline();
            List<string> lines = new List<string>();
            while(true)
            {
                string line = await readline();
                if (line.Trim() == ".")
                    break;
                lines.Add(line);
            }

            string body = string.Join("", lines);

            for (var i = 0; i < body.Length - 1; i++)
                if (body[i] == 10 && body[i + 1] == 46)
                    body = body.Substring(0, i) + body.Substring(i + 2);

            body = body.Replace("\x0a", "");
            body = body.Replace("\x0d", "");
            body = body.Replace("=C", "\x0a");
            body = body.Replace("=B", "\x0d");
            body = body.Replace("=A", "\x00");
            body = body.Replace("=D", "=");

            return body;
        }


        private async Task<string> readline()
        {
            byte[] buffer = new byte[10240];
            int len = 0;
            while (true)
            {
                await stream.ReadAsync(buffer, len, 1);
                if (buffer[len] == '\n')
                    break;
                len++;
            }
            return Encoding.GetEncoding("ISO-8859-1").GetString(buffer, 0, len+1);
        }

        private async Task Send(string data)
        {
            byte[] d = Encoding.GetEncoding("ISO-8859-1").GetBytes(data);
            await stream.WriteAsync(d, 0, d.Length);
/*            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Send: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(data.Trim());*/
/*            byte[] d = Encoding.GetEncoding("ISO-8859-1").GetBytes(data);
            for (int i = 0; i < d.Length; i+=1)
            {
                await stream.WriteAsync(d, i, 1);
                await Task.Delay(1);
            }*/

        }
    }
}
