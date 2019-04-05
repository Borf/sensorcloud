using P1Meter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.P1Meter
{
    public partial class Service : SensorCloud.Service
    {
        private Config config;
        private SmartMeter meter;

        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            meter = new SmartMeter();
            meter.OnData += OnData;
            meter.Connect(config.serial);
            return Task.CompletedTask;
        }

        private async void OnData(object sender, DataPacket data)
        {
            if(mqtt != null)
            {
                await mqtt.Publish("p1/power1", data.PowerConsumptionTariff1 + "");
                await mqtt.Publish("p1/power2", data.PowerConsumptionTariff2 + "");
                await mqtt.Publish("p1/gas", data.GasUsage + "");
            }
        }
    }
}
