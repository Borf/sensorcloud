using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	//TODO: this is just a temporary class until a proper action builder is in
	public class SensorCloudTelegramModule : Module
	{
		private TelegramModule telegram;
		private MqttModule mqtt;

		public override void Start()
		{
			telegram = GetModule<TelegramModule>();
			mqtt = GetModule<MqttModule>();

			Menu projectorMenu = telegram.GetRootMenu("Projector");
			if(projectorMenu != null)
			{
				new Menu("Projector screen up", async () => await mqtt.Publish("livingroom/RF/7", "up"), projectorMenu);
				new Menu("Projector screen down", async () => await mqtt.Publish("livingroom/RF/7", "down"), projectorMenu);
				new Menu("Projector screen stop", async () => await mqtt.Publish("livingroom/RF/7", "stop"), projectorMenu);
			}

		}

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
