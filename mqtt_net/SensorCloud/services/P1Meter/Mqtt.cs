﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.P1Meter
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
