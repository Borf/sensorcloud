using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SensorCloud_test
{
    class Program
    {
        static void Main(string[] args)
        {
            //new UnitTest1().TestMethod1();
            //System.Diagnostics.Process.Start(@"c:\Program Files\IrfanView\i_view64.exe", "test.png");

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddJsonFile(Path.GetFullPath("../../../../appsettings.json")); //ewww
            IConfiguration configuration = builder.Build();

            SensorCloud.services.dash.Config config = new SensorCloud.services.dash.Config();
            configuration.GetSection("Dashboard").Bind(config);

            var db = new SensorCloudContext(configuration);

            var service = new SensorCloud.services.dash.Service(null, configuration, config);

            service.handleSshCommand(db.dashboardItems.First(i => i.type == "sshcommand"));

        }
    }
}
