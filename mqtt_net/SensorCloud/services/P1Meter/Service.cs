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

        public Decimal powerOneMinute { get; private set; }
        public Decimal powerTenMinute { get; private set; }

        public Decimal gasOneMinute { get; private set; }
        public Decimal gasTenMinute { get; private set; }


        public Service(IServiceProvider services, IConfiguration configuration, Config config) : base(services)
        {
            this.config = config;
            this.configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            db = new SensorCloudContext(configuration);

            meter = new SmartMeter();
            meter.OnData += OnData;
            meter.Connect(config.serial);
            return Task.CompletedTask;
        }

        private async void OnData(object sender, DataPacket data)
        {
            measurements.Add(new Measurement() { data = data, time = DateTime.Now });
            while (measurements.Count > 0 && measurements[0].time.AddMinutes(10) < DateTime.Now)
                measurements.RemoveAt(0);

            db.sensordata.Add(new SensorData()
            {
                stamp = DateTime.Now,
                type = "power1",
                value = (double)data.PowerConsumptionTariff1
            });
            db.sensordata.Add(new SensorData()
            {
                stamp = DateTime.Now,
                type = "power2",
                value = (double)data.PowerConsumptionTariff2
            });
            db.sensordata.Add(new SensorData()
            {
                stamp = DateTime.Now,
                type = "gas",
                value = (double)data.GasUsage
            });

            if (measurements.Count > 0)
            {
                DataPacket minuteOne = measurements.AsEnumerable().Reverse().TakeWhile(m => m.time.AddMinutes(1) >= DateTime.Now).Last().data;
                DataPacket minuteTen = measurements.AsEnumerable().Reverse().TakeWhile(m => m.time.AddMinutes(10) >= DateTime.Now).Last().data;

                powerOneMinute = (data.PowerConsumptionTariff1 - minuteOne.PowerConsumptionTariff1) + (data.PowerConsumptionTariff2 - minuteOne.PowerConsumptionTariff2);
                powerTenMinute = (data.PowerConsumptionTariff1 - minuteTen.PowerConsumptionTariff1) + (data.PowerConsumptionTariff2 - minuteTen.PowerConsumptionTariff2);

                gasOneMinute = data.GasUsage - minuteOne.GasUsage;
                gasTenMinute = data.GasUsage - minuteTen.GasUsage;
            }
            if (mqtt != null)
            {
                await mqtt.Publish("p1/power1", data.PowerConsumptionTariff1 + "");
                await mqtt.Publish("p1/power2", data.PowerConsumptionTariff2 + "");
                await mqtt.Publish("p1/power", data.PowerConsumptionTariff1 + data.PowerConsumptionTariff2 + "");
                await mqtt.Publish("p1/gas", data.GasUsage + "");

                await mqtt.Publish("p1/use/1/power", powerOneMinute + "");
                await mqtt.Publish("p1/use/10/power", powerTenMinute + "");
                await mqtt.Publish("p1/use/1/gas", gasOneMinute + "");
                await mqtt.Publish("p1/use/10/gas", gasTenMinute + "");

            }
        }
    }
}
