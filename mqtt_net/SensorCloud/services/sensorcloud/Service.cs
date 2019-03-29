using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SensorCloud.services.telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.sensorcloud
{
    public class Service : SensorCloud.Service
    {
        private mqtt.Service mqtt;
        private SensorCloudContext db;
        private IConfiguration configuration;

        public Service(IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.configuration = configuration;
        }

        public override void InstallTelegramHandlers(telegram.Service telegram)
        {//TODO: softcode this
            Menu projectorMenu = telegram.GetRootMenu("Projector");
            if (projectorMenu != null)
            {
                new Menu("Projector screen up", async () => await mqtt.Publish("livingroom/RF/7", "up"), projectorMenu);
                new Menu("Projector screen down", async () => await mqtt.Publish("livingroom/RF/7", "down"), projectorMenu);
                new Menu("Projector screen stop", async () => await mqtt.Publish("livingroom/RF/7", "stop"), projectorMenu);
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqtt = GetService<mqtt.Service>();
            db = new SensorCloudContext(configuration);

            mqtt.On("boot/whoami$", async (match, message) =>
            {
                dynamic data = JObject.Parse(message);
                String hwid = data.hwid;

                var ret = db.nodes.Where(n => n.hwid == hwid).Select(n => new
                {
                    id = n.id,
                    hwid = n.hwid,
                    name = n.name,
                    topic = n.topic,
                    room = n.roomId,
                    config = JsonConvert.DeserializeObject(n.config),
                    roomtopic = n.Room.topic,
                    sensors = n.sensors.Select(s => new
                    {
                        id = s.id,
                        nodeid = s.nodeid,
                        type = s.type,
                        config = JsonConvert.DeserializeObject(s.config)
                    })
                }).First();
                await mqtt.Publish("boot/whoami/" + hwid, JsonConvert.SerializeObject(ret));
                Log($"Sensor {ret.name}  (id {ret.id}) in {ret.roomtopic} is booting");
                new Node(hwid, services, configuration);
            });

            mqtt.On("boot/whoami/(.+)", (match, message) =>
            {

            });




            return Task.CompletedTask;
        }
    }
}
