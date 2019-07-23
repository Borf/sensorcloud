using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SensorCloud_test
{
    [TestClass]
    class P1MeterTest
    {
        [TestMethod]
        public void TestHistory()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.GetFullPath("../../../../appsettings.json")); //ewww
            IConfiguration configuration = builder.Build();

            var config = new SensorCloud.services.P1Meter.Config();
            configuration.GetSection("P1Meter").Bind(config);


            var service = new SensorCloud.services.P1Meter.Service(null, configuration, config);

            var bla = service.StartAsync(new System.Threading.CancellationToken());
        }
    }
}