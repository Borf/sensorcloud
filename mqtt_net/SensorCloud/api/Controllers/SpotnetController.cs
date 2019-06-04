using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SensorCloud.services.spotnet;
using SensorCloud.services.spotnet.xml.caps;
using SensorCloud.services.spotnet.xml.rss;
using SpotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]

    public class SpotnetController : Controller
    {
        private SensorCloudContext db;
        private services.spotnet.Config config;

        public SpotnetController(SensorCloudContext db, services.spotnet.Config config)
        {
            this.db = db;
            this.config = config;
        }

        [HttpGet("api")]
        [AllowAnonymous]
        [Produces("application/xml")]
        public async Task<object> ApiAsync(  [FromQuery(Name = "t")] string query,
                            [FromQuery(Name = "key")] string key,
                            [FromQuery(Name = "q")] string q,
                            [FromQuery(Name = "season")] string season,
                            [FromQuery(Name = "ep")] string ep,
                            [FromQuery(Name = "rid")] int rageid = -1
            )
        {
            if(query == "caps")
            {
                return new Caps()
                {
                    Server = new Server()
                    {
                        Version = "1.0",
                        Title = "Spotstream",
                        Strapline = "A great usenet indexer",
                        Email = "none@example.com",
                        Url = "http://servername.com/",
                        Image = "http://servername.com/theme/black/images/banner.jpg"
                    },
                    Limits = new Limits()
                    {
                        Default = "500",
                        Max = "1000"
                    },
                    Retention = new Retention() { Days = "400" },
                    Registration = new Registration()
                    {
                        Available = "yes",
                        Open = "no"
                    },
                    Searching = new Searching()
                    {
                        Search = new Search() { Available = "yes" },
                        Tvsearch = new Tvsearch() { Available = "yes" },
                        Moviesearch = new Moviesearch() { Available = "yes" }
                    },
                    Categories = new Categories()
                    {
                        Category = new List<Category>()
                        {
                            new Category()
                            {
                                Id = "1000",
                                Name = "Console",
                                Subcat = new List<Subcat>()
                                {
                                    new Subcat() {  Id = "1010",    Name = "NDS" },
                                    new Subcat() {  Id = "1020",    Name = "PSP" },
                                }
                            },
                            new Category()
                            {
                                Id = "2000",
                                Name = "Movies",
                                Subcat = new List<Subcat>()
                                {
                                    new Subcat() {  Id = "2010",    Name = "Foreign" },
                                }
                            },
                            new Category()
                            {
                                Id = "5030",
                                Name = "TV/SD",
                            },
                            new Category()
                            {
                                Id = "5040",
                                Name = "TV/HD",
                            },
                        }
                    }
                };
            }
            if(query == "tvsearch" || query == "search")
            {
                IQueryable<datamodel.Spot> spots = db.spots.OrderByDescending(s => s.posted);

                if(rageid != -1)
                {
                    WebRequest request = WebRequest.Create($"http://api.tvmaze.com/lookup/shows?tvrage={rageid}");
                    WebResponse response = await request.GetResponseAsync();
                    string value = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
//{"id":4,"url":"http://www.tvmaze.com/shows/4/arrow","name":"Arrow","type":"Scripted","language":"English","genres":["Drama","Action","Science-Fiction"],"status":"Running","runtime":60,"premiered":"2012-10-10","officialSite":"http://www.cwtv.com/shows/arrow","schedule":{"time":"21:00","days":["Tuesday"]},"rating":{"average":7.5},"weight":100,"network":{"id":5,"name":"The CW","country":{"name":"United States","code":"US","timezone":"America/New_York"}},"webChannel":null,"externals":{"tvrage":30715,"thetvdb":257655,"imdb":"tt2193021"},"image":{"medium":"http://static.tvmaze.com/uploads/images/medium_portrait/165/414895.jpg","original":"http://static.tvmaze.com/uploads/images/original_untouched/165/414895.jpg"},"summary":"<p>After a violent shipwreck, billionaire playboy Oliver Queen was missing and presumed dead for five years before being discovered alive on a remote island in the Pacific. He returned home to Starling City, welcomed by his devoted mother Moira, beloved sister Thea and former flame Laurel Lance. With the aid of his trusted chauffeur/bodyguard John Diggle, the computer-hacking skills of Felicity Smoak and the occasional, reluctant assistance of former police detective, now beat cop, Quentin Lance, Oliver has been waging a one-man war on crime.</p>","updated":1558008839,"_links":{"self":{"href":"http://api.tvmaze.com/shows/4"},"previousepisode":{"href":"http://api.tvmaze.com/episodes/1620103"}}}
                    JObject ret = JObject.Parse(value);
                    spots = spots.Where(s => s.title.Contains(ret["name"].ToObject<string>()));
                }
                else if(q != null)
                {
                    spots = spots.Where(s => s.title.Contains(q));
                }

                if (season != "")
                    spots = spots.Where(s => s.title.Contains($"s{season}") || s.title.Contains($"s0{season}"));
                if (ep != "")
                    spots = spots.Where(s => s.title.Contains($"e{ep}") || s.title.Contains($"e0{ep}"));

                spots = spots.Take(1000);





                return new Rss()
                {
                    Channel = new Channel()
                    {
                        Title = "Spotnet results",
                        Description = "Spot results",
                        Response = new Response()
                        {
                            Offset = "0",
                            Total = "1000"
                        },
                        Item = spots.Select(s => new Item()
                        {
                            Title = s.title,
                            Guid = new SensorCloud.services.spotnet.xml.rss.Guid()
                            {
                                IsPermaLink = "true",
                                Text = $"http://10.10.0.192:5353/spotnet/nzb/{s.article}"
                            },
                            Link = $"http://10.10.0.192:5353/spotnet/nzb/{s.article}",
                            PubDate = s.posted.ToPubDate(),
                            Enclosure = new Enclosure()
                            {
                                Type = "application/x-nzb",
                                Url = $"http://10.10.0.192:5353/spotnet/nzb/{s.article}",
                                Length = s.size
                            },
                            Attr = new List<Attr>()
                            {
                                new Attr()
                                {
                                    Name = "category",
                                    Value = 5040
                                },
                                new Attr()
                                {
                                    Name = "size",
                                    Value = s.size
                                }
                            }
                        }).ToList()
                    }
                };
            }

            return "";
        }

        [HttpGet("nzb/{id}")]
        [AllowAnonymous]
        [Produces("text/poop")] //text/plain doesn't work :(
        public async Task<object> Nzb(int id)
        {
            var nzbs = db.spotNzbs.Where(e => e.article == id).ToList();
            using (Spotnet spotnet = new Spotnet(config.host, config.port, config.user, config.pass))
            {
                await spotnet.Connect();
                var data = await spotnet.Nzb(nzbs[0].segment);
                return data;
            }
        }


    }
}
