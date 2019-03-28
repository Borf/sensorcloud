using JvcProjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.jvc
{
    public partial class Service : SensorCloud.Service
    {
        private Projector projector;
        private Config config;


        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            projector = new Projector();
            projector.StatusChange += onStatusChange;
            await projector.Connect(config.host);
        }


        private async void onStatusChange(object sender, PowerStatus e)
        {
            mqtt?.Publish("projector/power", e.ToString(), retain: true);
            if (telegram?.IsInMenu(projectorMenu) == true)
                await telegram.SendMessageAsync($"Power changed to {e.ToString()}");

        }
    }
}
