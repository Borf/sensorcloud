using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.sensorcloud
{
    public class Node : SensorCloud.Service
    {
        private mqtt.Service mqtt;
        private IConfiguration configuration;

        public datamodel.Node node { get; private set; }
        public string hwid {get; private set; }
        public DateTime lastPing { get; private set; }
        public DateTime lastValue { get; private set; }


        public Node(string hwid, IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.mqtt = GetService<mqtt.Service>();
            this.configuration = configuration;
            using (var db = new SensorCloudContext(configuration))
                node = db.nodes.Include(node => node.Room).Include(node => node.sensors).First(n => n.hwid == hwid);
            mqtt.On("(" + node.Room.topic + "/" + node.topic + ")/(.*)", onData);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        private void onData(Match match, string payload)
        {
            if (match.Groups[2].Value == "ping")
            {
                dynamic data = JObject.Parse(payload);
                Ping ping = new Ping()
                {
                    stamp = DateTime.Now,
                    nodeid = node.id,
                    ip = data.ip,
                    heapspace = data.heapspace,
                    rssi = data.rssi
                };
                using (var db = new SensorCloudContext(configuration))
                {
                    db.pings.Add(ping);
                    db.SaveChanges();
                }
                lastPing = DateTime.Now;
            }
            if (match.Groups[2].Value == "temperature" || match.Groups[2].Value == "humidity")
            {
                if (payload == "0")
                    return;
                try
                {
                    SensorData sensorData = new SensorData()
                    {
                        stamp = DateTime.Now,
                        nodeid = node.id,
                        type = match.Groups[2].Value,
                        value = float.Parse(payload)
                    };
                    using (var db = new SensorCloudContext(configuration))
                    {
                        db.sensordata.Add(sensorData);
                        db.SaveChanges();
                    }
                    lastValue = DateTime.Now;
                }
                catch (FormatException)
                {
                    Log("Error while parsing payload");
                }
            }

            if (match.Groups[2].Value == "alive")
            {
                if (payload == "dead")
                {
                    Log("Node is dead, removing from listening list");
                    mqtt.Un("(" + node.Room.topic + "/" + node.topic + ")/(.*)");
                    Dispose(); //TODO: check
                }
            }

        }
    }
}
