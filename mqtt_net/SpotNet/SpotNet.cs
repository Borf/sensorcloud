using NNTP;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SpotNet
{
    public class Spotnet : IDisposable
    {
        Connection nntp = new Connection();
        private string host;
        private int port;
        private string user;
        private string pass;
        public Group GroupInfo;

        public Spotnet(string host, int port, string user, string pass)
        {
            this.host = host;
            this.port = port;
            this.user = user;
            this.pass = pass;
        }

        public event EventHandler<Spot> OnSpot;


        public async Task Connect()
        {
            await nntp.Connect(host, port, user, pass);
            GroupInfo = await nntp.Group("free.pt");
        }

        public void Disconnect()
        {
            nntp.Disconnect();
        }

        public async Task<long> Update(long start)
        {
            GroupInfo = await nntp.Group("free.pt");
            for (long id = start; id <= GroupInfo.high; id++)
            {
                await Get(id);
                if(id % 50 == 0)
                    System.GC.Collect();
            }
            return GroupInfo.high+1;
        }

        public async Task Get(long id)
        {
            Header h = await nntp.Headers(id);
            if (h != null && h.headers.ContainsKey("X-XML"))
            {
                try
                {
                    Spot spot = new Spot(h);
                    if (spot.segments != null)
                        OnSpot?.Invoke(this, spot);
                }
                catch (XmlException)
                {
                    //  Console.WriteLine("Could not parse spot: " + e + "\n\n" + h.headers["X-XML"]);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception thrown while parsing spot: {id}, {e}");
                }
            }
            else
            { /*    Console.WriteLine($"{id} not a spotnet spot");*/ }
        }


        public async Task<string> Nzb(Spot spot)
        {
            return await Nzb(spot.segments[0]);
        }

        public async Task<string> Nzb(string segment)
        {
            await nntp.Group("alt.binaries.ftd");
            var data = await nntp.Body("<" + segment + ">");
            var reader = new StreamReader(new DeflateStream(new MemoryStream(Encoding.GetEncoding("ISO-8859-1").GetBytes(data)), CompressionMode.Decompress));
            var output = await reader.ReadToEndAsync();
            return output;
        }

        public void Dispose()
        {
            nntp.Dispose();
        }
    }
}
