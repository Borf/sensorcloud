using api;
using API;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json.Linq;
using SensorCloud.modules;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud
{
	class Program
	{
		static JArray config;

		static void Main(string[] args)
		{
			config = JArray.Parse(File.ReadAllText("config.json"));

			foreach (JObject moduleConfig in config)
				buildModule(moduleConfig);

			ModuleManager.StartAll();

			CreateWebHostBuilder(args).Build().Run();

			Thread.Sleep(Timeout.Infinite);
		}

		private static void buildModule(JObject moduleConfig)
		{
			switch (moduleConfig["name"].ToString())
			{
				case "onkyo":
					new OnkyoModule(moduleConfig["host"].Value<string>());
					break;
				case "mqtt":
					new MqttModule(moduleConfig["broker"].Value<string>());
					break;
				case "telegram":
					new TelegramModule(moduleConfig["bottoken"].Value<string>(), moduleConfig["chatid"].Value<int>());
					break;
				case "mysql":
					new MysqlModule(
						host: moduleConfig["host"].Value<string>(),
						port: moduleConfig["port"].Value<int>(),
						user: moduleConfig["user"].Value<string>(),
						pass: moduleConfig["pass"].Value<string>(),
						daba: moduleConfig["daba"].Value<string>()
						);
					break;
				case "sensorcloudboot":
					new SensorCloudBootModule();
					break;
				case "dashboard":
					new DashModule();
					break;
			}


		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
			.UseStartup<Startup>();
	}
}
