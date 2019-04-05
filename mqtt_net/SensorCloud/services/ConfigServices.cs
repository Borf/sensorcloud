﻿using System.Collections.Generic;

namespace SensorCloud.services
{
    class ConfigServices
    {
        public static List<IConfigServiceBase> services = new List<IConfigServiceBase>
        {
            new ConfigService<mqtt.Service, mqtt.Config>("Mqtt"),
            new ConfigService<sensorcloud.Service, sensorcloud.Config>("Sensorcloud"),
            new ConfigService<onkyo.Service, onkyo.Config>("Onkyo"),
            new ConfigService<kodi.Service, kodi.Config>("Kodi"),
            new ConfigService<jvc.Service, jvc.Config>("Jvc"),
            new ConfigService<P1Meter.Service, P1Meter.Config>("P1Meter"),
            new ConfigService<dash.Service, dash.Config>("Dashboard"),



            new ConfigService<telegram.Service, telegram.Config>("Telegram"),
        };
    }
}
