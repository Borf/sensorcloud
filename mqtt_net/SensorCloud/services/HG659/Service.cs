using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.HG659
{
    public partial class Service : SensorCloud.Service
    {
        private Config config;
        private IConfiguration configuration;
        private Session session;

        public Service(IServiceProvider services, IConfiguration configuration, Config config) : base(services)
        {
            this.config = config;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            session = new Session(config);
            await session.Login();

            while (true)
            {
                await SenseDeviceCount();
                await CheckNewHosts();

                await Task.Delay(config.interval * 1000);
            }
        }

        public async Task<List<Host>> GetHosts()
        {
            var hosts = await session.getApi("api/system/HostInfo");
            return hosts.ToObject<List<Host>>();
        }


        private async Task CheckNewHosts()
        {
            List<Host> currentHosts = await GetHosts();
            using (SensorCloudContext db = new SensorCloudContext(configuration))
            {
                var dbHosts = db.HG569_hosts.ToList();
                foreach(var host in currentHosts)
                {
                    if (dbHosts.Any(h => h.hostname == host.HostName && h.ip == host.IPAddress && h.mac == host.MACAddress))
                        continue;
                    var dbHost = dbHosts.FirstOrDefault(h => h.mac == host.MACAddress);

                    if (dbHost == null)
                        await db.HG569_hosts.AddAsync(new HG659_host()
                        {
                            mac = host.MACAddress,
                            hostname = host.HostName,
                            ip = host.IPAddress
                        });
                    else
                    {
                        if (dbHost.hostname != host.HostName)
                            Console.WriteLine($"hostname changed for mac {host.MACAddress}, host {host.HostName}");
                        if (dbHost.ip != host.IPAddress)
                            Console.WriteLine($"IP address changed for mac {host.MACAddress}, host {host.HostName}");
                        dbHost.ip = host.IPAddress;
                        dbHost.hostname = host.HostName;
                    }

                }
                await db.SaveChangesAsync();
            }


        }



        private async Task SenseDeviceCount()
        {
            using (SensorCloudContext db = new SensorCloudContext(configuration))
            {

                var deviceCount = (JObject)(await session.getApi("api/system/device_count"));
                int max = 0;
                db.sensordata.Add(new SensorData()
                {
                    stamp = DateTime.Now,
                    nodeid = 0,
                    type = "NetworkClients",
                    value = (int)deviceCount["ActiveDeviceNumbers"]
                });
                db.sensordata.Add(new SensorData()
                {
                    stamp = DateTime.Now,
                    nodeid = 0,
                    type = "NetworkWired",
                    value = (int)deviceCount["LanActiveNumber"]
                });
                var wifiWizard = (JArray)(await session.getApi("api/system/wizard_wifi"));
                if (wifiWizard.Count > 0)
                {
                    max = wifiWizard.Max(w => (int)w["Numbers"]);
                    db.sensordata.Add(new SensorData()
                    {
                        stamp = DateTime.Now,
                        nodeid = 0,
                        type = "NetworkWifi",
                        value = max
                    });
                }
                await db.SaveChangesAsync();

                if (mqtt != null)
                {
                    await mqtt.Publish("network/clients", (int)deviceCount["ActiveDeviceNumbers"] + "");
                    await mqtt.Publish("network/clients/wired", (int)deviceCount["LanActiveNumber"] + "");
                    await mqtt.Publish("network/clients/wifi", max + "");
                }
            }//using
        }
    }
}
