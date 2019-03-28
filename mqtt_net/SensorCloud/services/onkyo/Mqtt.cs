using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SensorCloud.services.mqtt;

namespace SensorCloud.services.onkyo
{
    public partial class Service
    {
        private mqtt.Service mqtt;

        public override async void InstallMqttHandlers(mqtt.Service mqtt)
        {
            this.mqtt = mqtt;
            await mqtt.IsStarted();
            await mqtt.Publish("onkyo/status/title", "", retain: true);
            await mqtt.Publish("onkyo/status/album", "", retain: true);
            await mqtt.Publish("onkyo/status/artist", "", retain: true);


            mqtt.On("onkyo/volume/set", (match, payload) => Volume = int.Parse(payload));
            mqtt.On("onkyo/power/set", (match, payload) => Power = (payload == "on"));
            mqtt.On("onkyo/action", (match, payload) =>
            {
                if (payload == "next")
                {
                    Log("Skipping to next song");
                    receiver.Next();
                }
            });
            mqtt.On("onkyo/.*", (m, p) => { });
        }
    }
}
