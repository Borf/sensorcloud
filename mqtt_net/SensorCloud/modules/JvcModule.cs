using JvcProjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	public class JvcModule : Module
	{
		private Projector projector;
		private string host;

		private MqttModule mqtt;
		private TelegramModule telegram;
		private Menu projectorMenu;

		public JvcModule(string host)
		{
			this.host = host;
		}

		public override async void Start()
		{
			mqtt = GetModule<MqttModule>();
			telegram = GetModule<TelegramModule>();
			if (telegram != null)
				InstallTelegramHandlers(telegram);

			projector = new Projector();
			projector.StatusChange += onStatusChange;
			await projector.Connect(host);

		}

		private void InstallTelegramHandlers(TelegramModule telegram)
		{
			projectorMenu = new Menu(title: "Projector", afterMenuText: () => projector.Status.ToString());

			projectorMenu.Add(new Menu(title: "On", callback: () => projector.Status = PowerStatus.poweron));
			projectorMenu.Add(new Menu(title: "Off", callback: () => projector.Status = PowerStatus.standby));
			telegram.AddRootMenu(projectorMenu);
		}

		private void onStatusChange(object sender, PowerStatus e)
		{
			mqtt?.Publish("projector/power", e.ToString(), retain: true);

			if(telegram?.IsInMenu(projectorMenu) == true)
				telegram.SendMessageAsync($"Power changed to {e.ToString()}");

		}
	}
}
