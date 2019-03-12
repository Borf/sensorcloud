using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	public class KodiModule : Module
	{
		TelegramModule telegram;
		private Menu kodiMenu;

		private string host;

		public KodiModule(string host)
		{
			this.host = host;
		}

		public override void Start()
		{
			telegram = GetModule<TelegramModule>();
			if (telegram != null)
				InstallTelegramHandlers(telegram);

		}
		private void InstallTelegramHandlers(TelegramModule telegram)
		{
			kodiMenu = new Menu(title: "Kodi");

			kodiMenu.Add(new Menu(title: "Left", callback: async () => await CallRpc("Input.Left")));
			kodiMenu.Add(new Menu(title: "Right", callback: async () => await CallRpc("Input.Right")));
			kodiMenu.Add(new Menu(title: "Up", callback: async () => await CallRpc("Input.Up")));
			kodiMenu.Add(new Menu(title: "Down", callback: async () => await CallRpc("Input.Down")));
			kodiMenu.Add(new Menu(title: "Select", callback: async () => await CallRpc("Input.Select")));
			kodiMenu.Add(new Menu(title: "Back", callback: async () => await CallRpc("Input.Back")));
			telegram.AddRootMenu(kodiMenu);
		}

		int id = 1;


		private async Task CallRpc(string command)
		{
			//[{jsonrpc: "2.0", method: "Input.Down", params: [], id: 8}]
			HttpClient client = new HttpClient();
			await client.PostAsJsonAsync("http://192.168.2.22:8080/jsonrpc?" + command, new[]
			{
				new
				{
					jsonrpc = "2.0",
					method = command,
					//params = new int[] { },
					id = id
				}
			});
			id++;
		}
	}
}
