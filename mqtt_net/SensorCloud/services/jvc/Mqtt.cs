using SensorCloud.services.mqtt;

namespace SensorCloud.services.jvc
{
    public partial class Service
    {
        private mqtt.Service mqtt;

        public override void InstallMqttHandlers(mqtt.Service mqtt)
        {
            this.mqtt = mqtt;
        }
    }
}