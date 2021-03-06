﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SensorCloud.services.rulemanager;
using SensorCloud.services.telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SensorCloud.services.rulemanager.Service;

namespace SensorCloud.services.sensorcloud
{
    public partial class Service : SensorCloud.Service
    {
        private mqtt.Service mqtt;
        private SensorCloudContext db;
        private IConfiguration configuration;

        private List<Node> nodes = new List<Node>();

        public Service(IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqtt = GetService<mqtt.Service>();
            db = new SensorCloudContext(configuration); //TODO: move to a construction using 'using' keyword

            var ruleManager = GetService<rulemanager.Service>();
            ruleManager.AddFunction(new Function()
            {
                Module = this.moduleNameFirstCap,
                FunctionName = "Activate",
                Parameters = new List<Tuple<string, rules.Socket>>() { new Tuple<string, rules.Socket>("nodeid", new rules.NumberSocket()) },
                Callback = this.Activate
            });
            

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
                var newNode = new Node(hwid, services, configuration);
                if (nodes.Any(n => n.hwid == hwid))
                {
                    Log($"Sensor already connected before");
                    nodes.RemoveAll(n => n.hwid == hwid);
                }
                nodes.Add(newNode);

            });

            mqtt.On("boot/whoami/(.+)", (match, message) =>
            {

            });

            var watchDog = Task.Run(WatchDog);
            var backlog = Task.Run(BackLog);

            await Task.WhenAll(new Task[] { watchDog, backlog });
        }

        private async Task WatchDog()
        {
            List<Node> messagedNodes = new List<Node>();
            while(true)
            {
                await Task.Delay(1000);
                nodes.ForEach(async n =>
                {
                    bool isLate = (DateTime.Now - n.lastPing).TotalSeconds > 15*60 || (DateTime.Now - n.lastValue).TotalSeconds > 15*60;
                    if (isLate && !messagedNodes.Contains(n))
                    {
                        messagedNodes.Add(n);
                        if (telegram != null)
                            await telegram.SendMessageAsync($"Warning: node {n.node.name} in {n.node.Room.name} is missing in action!", true);
                    }
                    else if (!isLate && messagedNodes.Contains(n))
                        messagedNodes.Remove(n);
                });

            }
        }
        
        private async Task BackLog()
        {
            while (true)
            {
                await Task.Delay(1000 * 60 * 30);
                using (var db2 = new SensorCloudContext(configuration))
                {
                    Log("Updating backlog");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.hourly`  (`stamp`, `nodeid`, `type`, `value`) SELECT DATE_FORMAT(`stamp`, '%Y-%m-%d %H:00:00') as `date`, `nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `date`");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.daily`   (`stamp`, `nodeid`, `type`, `value`)  SELECT DATE_FORMAT(`stamp`, '%Y-%m-%d') as `date`, `nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `date`");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.weekly`  (`year`, `week`, `nodeid`, `type`, `value`)  SELECT year(`stamp`) as `year`, week(`stamp`) as `week`,`nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata.daily` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `stamp`, `year`, `week`");
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.monthly` (`year`, `month`, `nodeid`, `type`, `value`)  SELECT year(`stamp`) as `year`, month(`stamp`) as `month`,`nodeid`, `type`, round(avg(`value`), 2) FROM `sensordata.daily` WHERE `type` = 'TEMPERATURE' OR `type` = 'HUMIDITY' GROUP BY `nodeid`, `type`, `year`, `month`");

                    //aggregration for power/gas
                    await db2.Database.ExecuteSqlCommandAsync("REPLACE INTO `sensordata.hourly`  (`stamp`, `nodeid`, `type`, `value`) SELECT DATE_FORMAT(`stamp`, '%Y-%m-%d %H:00:00') as `date`, `nodeid`, `type`, max(`value`) - min(`value`) as `value` FROM `sensordata` WHERE `type` = 'power1' OR `type` = 'power2' OR `type` = 'gas' GROUP BY `nodeid`, `type`, `date`");

                    Log("Done updating backlog");
                }
            }
        }

        public void Activate(Dictionary<string, object> parameters)
        {

        }
    }
}
