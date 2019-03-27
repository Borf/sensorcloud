using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	public class SensorCloudBootModule : Module
	{
		MqttModule mqtt;
		SensorCloudContext db;

		public override void Start()
		{
			mqtt = GetModule<MqttModule>();
			db = GetModule<DbModule>().context;

			mqtt.On("boot/whoami$", async (match, message) =>
			{
				dynamic data = JObject.Parse(message);
				String hwid = data.hwid;

				var ret = db.nodes.Where(n => n.hwid == hwid).Select(n => new
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
				Log($"Sensor {ret.name}  (id {ret.id}) in {ret.roomtopic} is booting");
				new SensorCloudNodeModule(hwid).Start();
			});

			mqtt.On("boot/whoami/(.+)", (match, message) =>
			{

			});

		}

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
