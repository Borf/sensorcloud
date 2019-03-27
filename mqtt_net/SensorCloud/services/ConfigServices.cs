using System.Collections.Generic;

namespace SensorCloud.services
{
    class ConfigServices
    {
        public static List<IConfigServiceBase> services = new List<IConfigServiceBase>
        {
            new ConfigService<mqtt.Service, mqtt.Config>("Mqtt"),
            new ConfigService<onkyo.Service, onkyo.Config>("Onkyo"),
            new ConfigService<kodi.Service, kodi.Config>("Kodi"),



            new ConfigService<telegram.Service, telegram.Config>("Telegram"),
        };
    }
}
