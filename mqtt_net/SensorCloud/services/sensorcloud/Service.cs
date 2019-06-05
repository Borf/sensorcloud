﻿using Microsoft.EntityFrameworkCore;
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
    public partial class Service : SensorCloud.Service
    {
        private mqtt.Service mqtt;
        private SensorCloudContext db;
        private IConfiguration configuration;

        public Service(IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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



            while (true)
            {
                using (var db2 = new SensorCloudContext(configuration))
                {
                    Log("Updating backlog");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.hourly`  (`stamp`, `nodeid`, `type`, `value`) SELECT DATE_FORMAT(`stamp`, '%Y-%m-%d %H:00:00') as `date`, `nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `date`");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.daily`   (`date`, `nodeid`, `type`, `value`)  SELECT DATE_FORMAT(`stamp`, '%Y-%m-%d') as `date`, `nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `date`");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.weekly`  (`year`, `week`, `nodeid`, `type`, `value`)  SELECT year(`date`) as `year`, week(`date`) as `week`,`nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata.daily` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `date`, `year`, `week`");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.monthly` (`year`, `month`, `nodeid`, `type`, `value`)  SELECT year(`date`) as `year`, month(`date`) as `month`,`nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata.daily` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `year`, `month`");
                    
                    Log("Done updating backlog");
                }

                await Task.Delay(1000 * 60 * 30);

            }
        }
    }
}
