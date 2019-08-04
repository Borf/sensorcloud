using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SpotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.spotnet
{
    public partial class Service : SensorCloud.Service
    {
        private Config config;
        private IConfiguration configuration;


        public Service(Config config, IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.config = config;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                Spotnet spotnet = new Spotnet(config.host, config.port, config.user, config.pass);
                await spotnet.Connect();
                spotnet.OnSpot += async (s, spot) =>
                {
                    using (SensorCloudContext db = new SensorCloudContext(configuration))
                    {
                        Console.WriteLine($"Saving {spot.article}");
                        datamodel.Spot dbSpot = new datamodel.Spot()
                        {
                            article = spot.article,
                            articleid = spot.articleid,
                            cat = spot.cat,
                            desc = spot.desc,
                            posted = spot.posted,
                            size = spot.size,
                            subcat = spot.subcat,
                            title = spot.title,
                            nzbs = spot.segments.Select(seg => new datamodel.SpotNzb()
                            {
                                article = spot.article,
                                articleid = spot.articleid,
                                segment = seg
                            }).ToList()
                        };
                        db.spots.Add(dbSpot);
                        db.spotNzbs.AddRange(dbSpot.nzbs);
                        await db.SaveChangesAsync();
                        if (mqtt != null)
                            await mqtt.Publish("spotnet", JObject.FromObject(new { id = spot.articleid, title = spot.title }).ToString());
                    }

                };
                long min = spotnet.GroupInfo.low;
                using (SensorCloudContext db = new SensorCloudContext(configuration))
                {
                    try
                    {
                        min = db.spots.Max(e => e.article) + 1;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                await spotnet.Update(min);

                spotnet.Disconnect();
                await Task.Delay(60000*5);
            }
        }

    }
}
