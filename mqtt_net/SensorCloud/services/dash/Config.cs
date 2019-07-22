using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.dash
{
    public class Config
    {
        public class HostConfig
        {
            public string Host { get; set; }
            public string User { get; set; }
            public string Pass { get; set; }
        }

        public Dictionary<String, HostConfig> hosts { get; set; }


    }
}
