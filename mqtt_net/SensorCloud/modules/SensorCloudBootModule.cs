using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	public class SensorCloudBootModule : Module
	{
		MqttModule mqtt;
		SensorCloudContext mysql;

		public override void Start()
		{
			mqtt = GetModule<MqttModule>();
			mysql = GetModule<MysqlModule>().context;

			mqtt.On("boot/whoami$", async (match, message) =>
			{
				dynamic data = JObject.Parse(message);
				String hwid = data.hwid;

				var ret = mysql.nodes.Where(n => n.hwid == hwid).Select(n => new
				{
					id = n.id,
					hwid = n.hwid,
					name = n.name,
					topic = n.topic,
					room = n.roomId,
					config = JsonConvert.DeserializeObject(n.config),
					roomtopic = n.Room.topic,
					sensors = n.sensors.Select(s => new
					{
						id = s.id,
						nodeid = s.nodeid,
						type = s.type,
						config = JsonConvert.DeserializeObject(s.config)
					})
				}).First();
				await mqtt.Publish("boot/whoami/" + hwid, JsonConvert.SerializeObject(ret));

				new SensorCloudNodeModule(hwid).Start();
			});

			mqtt.On("boot/whoami/(.+)", (match, message) =>
			{
				
			});

		}
	}
}
