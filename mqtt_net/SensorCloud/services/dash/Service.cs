using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.dash
{
    public partial class Service : SensorCloud.Service
    {
        private SensorCloudContext db;
        private mqtt.Service mqtt;
        private IConfiguration configuration;

        public Service(IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqtt = GetService<mqtt.Service>();
            db = new SensorCloudContext(configuration);

            foreach (var item in db.dashboardItems.ToList())
                if (item.type == "mqtt")
                    StartMqtt(item);

            Task.Run(async () => await update());


            return Task.CompletedTask;
        }

        async Task update()
        {
            while (true)
            {
                Log("Running a new update cycle");
                foreach (var item in db.dashboardItems.ToList())
                {
                    switch (item.type)
                    {
                        case "ping": handlePing(item); break;
                        case "pingtime": handlePingTime(item); break;
                        case "get": await handleGetAsync(item); break;
                        case "mqtt": HandleMqtt(item); break;
                        case "http": await handleHttpAsync(item); break;
                        case "socket":
                        case "sensor":

                        case "image":
                        case "":
                            break;
                        default:
                            Log($"Unknown item type: {item.type}");
                            continue;
                    }
                    item.time = DateTime.Now;
                    await Task.Delay(100);
                }
                await db.SaveChangesAsync();
                await Task.Delay(10000);
            }
        }









 

      

    }
}
