using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SensorCloud.rules;
using SensorCloud.services.HG659;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SensorCloud_test
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            //new UnitTest1().TestMethod1();
            //System.Diagnostics.Process.Start(@"c:\Program Files\IrfanView\i_view64.exe", "test.png");

            //new P1MeterTest().TestHistory();

            //            Engine.init(null);
            //            var engine = new Engine(@"{""id"":""demo@0.1.0"",""nodes"":{""1"":{""id"":1,""data"":{},""inputs"":{},""outputs"":{""text"":{""connections"":[]},""trigger"":{""connections"":[{""node"":2,""input"":""trigger"",""data"":{}}]}},""position"":[187.1567602452404,403.0673120361407],""name"":""Receive Telegram Message""},""2"":{""id"":2,""data"":{},""inputs"":{""text"":{""connections"":[{""node"":3,""output"":""text"",""data"":{}}]},""trigger"":{""connections"":[{""node"":1,""output"":""trigger"",""data"":{}}]}},""outputs"":{},""position"":[957.76488625829,252.0673892890252],""name"":""Send Telegram Message""},""3"":{""id"":3,""data"":{""text"":""Hoi""},""inputs"":{},""outputs"":{""text"":{""connections"":[{""node"":2,""input"":""text"",""data"":{}}]}},""position"":[447,126],""name"":""Text""}}}");
            //            engine.trigger("Receive Telegram Message", new Dictionary<string, object>() { { "name", "Hello" } });


            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.GetFullPath("../../../../appsettings.json")); //ewww
            IConfiguration configuration = builder.Build();

            var config = new SensorCloud.services.HG659.Config();
            configuration.GetSection("HG659").Bind(config);


            var service = new SensorCloud.services.HG659.Service(null, configuration, config);

            var bla = service.StartAsync(new System.Threading.CancellationToken());

            Console.ReadKey();

        }

    }
}
