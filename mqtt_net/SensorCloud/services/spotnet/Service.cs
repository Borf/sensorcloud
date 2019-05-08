using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using SpotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.spotnet
{
    public class Service : SensorCloud.Service
    {
        private Config config;
        private SensorCloudContext db;
        private IConfiguration configuration;


        public Service(Config config, IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.config = config;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            db = new SensorCloudContext(configuration);


            while (true)
            {
                Spotnet spotnet = new Spotnet(config.host, config.port, config.user, config.pass);
                await spotnet.Connect();
                spotnet.OnSpot += async (s, spot) =>
                {
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

                };

                long min = spotnet.GroupInfo.low;
                try
                {
                    min = db.spots.Max(e => e.article)+1;
                } catch(Exception e)
                {
                    Console.WriteLine(e);
                }

                await spotnet.Update(min);

                spotnet.Disconnect();
                await Task.Delay(1000);
            }
        }

    }
}
