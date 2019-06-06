using SensorCloud.services.telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.kodi
{
    public class Service : SensorCloud.Service
    {
        private Config config;

        private telegram.Menu kodiMenu;

        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log("Starting Kodi");
            return Task.CompletedTask;
        }

        public override void InstallTelegramHandlers(telegram.Service telegram)
        {
            kodiMenu = new Menu(title: "Kodi");

            kodiMenu.Add(new Menu(title: "Left", callback: async () => await CallRpc("Input.Left")));
            kodiMenu.Add(new Menu(title: "Right", callback: async () => await CallRpc("Input.Right")));
            kodiMenu.Add(new Menu(title: "Up", callback: async () => await CallRpc("Input.Up")));
            kodiMenu.Add(new Menu(title: "Down", callback: async () => await CallRpc("Input.Down")));
            kodiMenu.Add(new Menu(title: "Select", callback: async () => await CallRpc("Input.Select")));
            kodiMenu.Add(new Menu(title: "Return", callback: async () => await CallRpc("Input.Back")));
            telegram.AddRootMenu(kodiMenu);
        }

        int id = 1;
        private async Task CallRpc(string command)
        {
            //[{jsonrpc: "2.0", method: "Input.Down", params: [], id: 8}]
            try
            {
                HttpClient client = new HttpClient();
                await client.PostAsJsonAsync("http://192.168.2.22:8080/jsonrpc?" + command, new[]
                {
                    new
                    {
                        jsonrpc = "2.0",
                        method = command,
                        @params = new int[] { },
                        id = id
                    }
                });
                id++;
            } catch(System.Net.Sockets.SocketException e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
