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

        public Service(IServiceProvider services, IConfiguration configuration, Config config) : base(services)
        {
            this.config = config;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SensorCloudContext db = new SensorCloudContext(configuration);

            while (true)
            {
                {
                    WebRequest request = WebRequest.Create($"http://{config.address}/api/system/device_count");
                    WebResponse response = await request.GetResponseAsync();
                    string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                    data = data.Substring(data.IndexOf("/*") + 2);
                    data = data.Substring(0, data.LastIndexOf("*/"));
                    var deviceCount = JObject.Parse(data);
                    /*
                     * {
                          "PrinterNumbers": 0,
                          "UsbNumbers": 0,
                          "UserNumber": 2,
                          "LanActiveNumber": 10,
                          "PhoneNumber": 2,
                          "ActiveDeviceNumbers": 20
                        }
                     */
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

                    if (mqtt != null)
                    {
                        await mqtt.Publish("network/clients", (int)deviceCount["ActiveDeviceNumbers"] + "");
                        await mqtt.Publish("network/clients/wired", (int)deviceCount["LanActiveNumber"] + "");
                    }
                }
                {
                    WebRequest request = WebRequest.Create($"http://{config.address}/api/system/wizard_wifi");
                    WebResponse response = await request.GetResponseAsync();
                    string data = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                    data = data.Substring(data.IndexOf("/*") + 2);
                    data = data.Substring(0, data.LastIndexOf("*/"));
                    var wifiWizard = JArray.Parse(data);
                    if(wifiWizard.Count > 0)
                    {
                        int max = wifiWizard.Max(w => (int)w["Numbers"]);
                        db.sensordata.Add(new SensorData()
                        {
                            stamp = DateTime.Now,
                            nodeid = 0,
                            type = "NetworkWifi",
                            value = max
                        });
                        if(mqtt != null)
                        {
                            await mqtt.Publish("network/clients/wifi", max + "");
                        }
                    }
                }
                await db.SaveChangesAsync();




                await Task.Delay(config.interval * 1000);
            }
        }
    }
}
