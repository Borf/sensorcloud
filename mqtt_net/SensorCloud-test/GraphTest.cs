using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using SensorCloud.datamodel;
using System.IO;

namespace SensorCloud_test
{
    [TestClass]
    public class GraphTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.GetFullPath("../../../../appsettings.json")); //ewww

            IConfiguration configuration = builder.Build();
            

            var service = new SensorCloud.services.sensorcloud.Service(null, configuration);
            
            var bla = service.showSensorData("week", 1, "power", new List<Node>() { new Node() { id = 0 } });
            var reply = bla.Invoke();
            reply.image.Save("test.png");
            
        }
    }

}
