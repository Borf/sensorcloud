using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	public class SensorCloudNodeModule : Module
	{
		MqttModule mqtt;
		SensorCloudContext db;
		private Node node;
		string hwid;

		public SensorCloudNodeModule(string hwid)
		{
			this.hwid = hwid;
		}

		public override void Start()
		{
			mqtt = GetModule<MqttModule>();
			db = GetModule<DbModule>().context;

			node = db.nodes.Include(node => node.Room).Include(node => node.sensors).First(n => n.hwid == hwid);

			mqtt.On("(" + node.Room.topic + "/" + node.topic + ")/(.*)", onData);
		}

		private void onData(Match match, string payload)
		{
			if (match.Groups[2].Value == "ping")
			{
				dynamic data = JObject.Parse(payload);
				Ping ping = new Ping()
				{
					stamp = DateTime.Now,
					nodeid = node.id,
					ip = data.ip,
					heapspace = data.heapspace,
					rssi = data.rssi
				};
				db.pings.Add(ping);
				db.SaveChanges();
			}
			if (match.Groups[2].Value == "temperature" || match.Groups[2].Value == "humidity")
			{
				if (payload == "0")
					return;
				try
				{
					SensorData sensorData = new SensorData()
					{
						stamp = DateTime.Now,
						nodeid = node.id,
						type = match.Groups[2].Value,
						value = float.Parse(payload)
					};
					db.sensordata.Add(sensorData);
					db.SaveChanges();
				}
				catch (FormatException)
				{
					Log("Error while parsing payload");
				}
			}

			if (match.Groups[2].Value == "alive")
			{
				if (payload == "dead")
				{
					Log("Node is dead, removing from listening list");
					mqtt.Un("(" + node.Room.topic + "/" + node.topic + ")/(.*)");
					ModuleManager.Remove(this);
				}
			}

		}
	}
}
