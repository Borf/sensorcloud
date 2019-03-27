using api;
using API;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud
{
	class Program
	{
		static void Main(string[] args)
		{
            CreateWebHostBuilder(args).Build().Run();
        }


        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls(new string[] { "http://0.0.0.0:5353" })
            .UseWebRoot("SensorCloud/wwwroot")
            .ConfigureAppConfiguration((hostingcontext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange : true);
            })
			.UseStartup<Startup>();
	}
}
