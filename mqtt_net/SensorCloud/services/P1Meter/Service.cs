using Microsoft.Extensions.Configuration;
using P1Meter;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.P1Meter
{
    class Measurement
    {
        public DateTime time;
        public DataPacket data;
    }

    public partial class Service : SensorCloud.Service
    {
        private Config config;
        private SensorCloudContext db;
        private SmartMeter meter;

        List<Measurement> measurements = new List<Measurement>();
        private IConfiguration configuration;

        public Service(IServiceProvider services, IConfiguration configuration, Config config) : base(services)
        {
            this.config = config;
            this.configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            db = new SensorCloudContext(configuration);
            Log("Starting smart meter");
            meter = new SmartMeter();
            meter.OnData += OnData;
            meter.Connect(config.serial);
            Log("Started");
            return Task.CompletedTask;
        }

        private async void OnData(object sender, DataPacket data)
        {
            measurements.Add(new Measurement() { data = data, time = DateTime.Now });
            while (measurements.Count > 0 && measurements[0].time.AddMinutes(60*24) < DateTime.Now)
                measurements.RemoveAt(0);

            db.sensordata.Add(new SensorData()
            {
                stamp = DateTime.Now,
                nodeid = 0,
                type = "power1",
                value = (double)data.PowerConsumptionTariff1
            });
            db.sensordata.Add(new SensorData()
            {
                stamp = DateTime.Now,
                nodeid = 0,
                type = "power2",
                value = (double)data.PowerConsumptionTariff2
            });
            db.sensordata.Add(new SensorData()
            {
                stamp = DateTime.Now,
                nodeid = 0,
                type = "gas",
                value = (double)data.GasUsage
            });
            await db.SaveChangesAsync();


            if (mqtt != null)
            {

                var values = new Dictionary<String, int>
                {
                    { "1m", 1 },
                    { "10m", 10 },
                    { "1h", 60 },
                    { "24h", 60*24 },
                };
                if (measurements.Count > 0)
                {
                    foreach (var v in values)
                    {
                        DataPacket firstValue = measurements.AsEnumerable().Reverse().TakeWhile(m => m.time.AddMinutes(v.Value) >= DateTime.Now).Last().data;
                        decimal power = (data.PowerConsumptionTariff1 - firstValue.PowerConsumptionTariff1) + (data.PowerConsumptionTariff2 - firstValue.PowerConsumptionTariff2);
                        decimal gas = data.GasUsage - firstValue.GasUsage;
                        await mqtt.Publish($"p1/use/{v.Key}/power", power + "");
                        await mqtt.Publish($"p1/use/{v.Key}/gas", gas + "");
                    }
                }


                await mqtt.Publish("p1/power1", data.PowerConsumptionTariff1 + "");
                await mqtt.Publish("p1/power2", data.PowerConsumptionTariff2 + "");
                await mqtt.Publish("p1/power", data.PowerConsumptionTariff1 + data.PowerConsumptionTariff2 + "");
                await mqtt.Publish("p1/gas", data.GasUsage + "");
            }
        }
    }
}
