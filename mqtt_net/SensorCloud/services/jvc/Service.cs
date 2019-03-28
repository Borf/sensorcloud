using JvcProjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.jvc
{
    public class Service : SensorCloud.Service
    {
        private Projector projector;
        private Config config;

        private mqtt.Service mqtt;
        private telegram.Service telegram;
        private telegram.Menu projectorMenu;

        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqtt = GetService<mqtt.Service>();
            telegram = GetService<telegram.Service>();
            if (telegram != null)
                InstallTelegramHandlers();

            projector = new Projector();
            projector.StatusChange += onStatusChange;
            await projector.Connect(config.host);
        }

        private void InstallTelegramHandlers()
        {
            projectorMenu = new telegram.Menu(title: "Projector", afterMenuText: () => projector.Status.ToString());

            projectorMenu.Add(new telegram.Menu(title: "On", callback: () => projector.Status = PowerStatus.poweron));
            projectorMenu.Add(new telegram.Menu(title: "Off", callback: () => projector.Status = PowerStatus.standby));
            telegram.AddRootMenu(projectorMenu);
        }

        private async void onStatusChange(object sender, PowerStatus e)
        {
            mqtt?.Publish("projector/power", e.ToString(), retain: true);

            if (telegram?.IsInMenu(projectorMenu) == true)
                await telegram.SendMessageAsync($"Power changed to {e.ToString()}");

        }
    }
}
