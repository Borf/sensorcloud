using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	public class DashModule : Module
	{
		MqttModule mqtt;
		SensorCloudContext db;

		public override void Start()
		{
			mqtt = GetModule<MqttModule>();
			db = GetModule<DbModule>().context;

            foreach (var item in db.dashboardItems.ToList())
                if(item.type == "mqtt")
                    StartMqtt(item);

            Task.Run(async () => await update());
		}

		async Task update()
		{
			while (true)
			{
				Log("Running a new update cycle");
				foreach (var item in db.dashboardItems.ToList())
				{
					switch (item.type)
					{
						case "ping": handlePing(item); break;
						case "pingtime": handlePingTime(item); break;
						case "get": await handleGetAsync(item); break;
						case "mqtt": HandleMqtt(item); break;
						case "http": await handleHttpAsync(item); break;
						case "socket":
						case "sensor":

						case "image":
						case "":
							break;
						default:
							Log($"Unknown item type: {item.type}");
							continue;
					}
					item.time = DateTime.Now;
					await Task.Delay(100);
				}
				await db.SaveChangesAsync();
				await Task.Delay(10000);
			}
		}

		private async Task handleHttpAsync(DashboardItem item)
		{
			var parameters = item.parameter.Split("|");
			try
			{
				WebRequest request = WebRequest.Create(parameters[0]);
				WebResponse response = await request.GetResponseAsync();
				item.value = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
				if (item.value.Contains(parameters[1]))
					item.value = "ok";
				else
					item.value = "error";

			}
			catch (WebException e)
			{
				if (parameters[1] == "auth" && e.Message.Contains("401"))
					item.value = "ok";
				else
					item.value = "error";
			}

		}

        private void StartMqtt(DashboardItem item)
        {
            var options = item.parameter.Split("|");
            mqtt.storeLastValue(options[0]);
        }

        private void HandleMqtt(DashboardItem item)
		{
			var options = item.parameter.Split("|");
			mqtt.storeLastValue(options[0]);
			if (!mqtt.lastValues.ContainsKey(options[0]))
				item.value = "error";
			else
			{
				string value = mqtt.lastValues[options[0]];
				for (int i = 1; i < options.Length; i++)
				{
					var v = options[i].Split("=");
					if (value == v[0])
						item.value = v[1];
				}
			}
		}

		private async Task handleGetAsync(DashboardItem item)
		{
			try
			{
				WebRequest request = WebRequest.Create(item.parameter);
				WebResponse response = await request.GetResponseAsync();
				item.value = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
			}
			catch (WebException)
			{
				item.value = "error";
			}
		}

		private void handlePingTime(DashboardItem item)
		{
			AutoResetEvent waiter = new AutoResetEvent(false);
			var ping = new System.Net.NetworkInformation.Ping();
			ping.PingCompleted += (s, e) =>
			{
				if (e.Reply.Status == IPStatus.Success)
					item.value = e.Reply.RoundtripTime + "";
				else
					item.value = "error";
				waiter.Set();
			};
			ping.SendAsync(item.parameter, 100, null);
			waiter.WaitOne();
		}

		private void handlePing(DashboardItem item)
		{
			AutoResetEvent waiter = new AutoResetEvent(false);
			var ping = new System.Net.NetworkInformation.Ping();
			ping.PingCompleted += (s, e) =>
			{
				if (e.Reply != null && e.Reply.Status == IPStatus.Success)
					item.value = "ok";
				else
					item.value = "offline";
				waiter.Set();
			};
			ping.SendAsync(item.parameter, 100, null);
			waiter.WaitOne();
		}
	}
}
