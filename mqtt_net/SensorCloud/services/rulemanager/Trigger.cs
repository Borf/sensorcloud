using Newtonsoft.Json;
using SensorCloud.rules;
using System;
using System.Collections.Generic;

namespace SensorCloud.services.rulemanager
{
    public partial class Service
    {
        public class Trigger
        {
            public string Module { get; set; }
            public string TriggerName { get; set; }
            public List<Tuple<string, Socket>> Inputs { get; set; }
            public List<Tuple<string, Socket>> Outputs { get; set; }

            [JsonIgnore]
            public Func<Node, bool> Callback { get; set; }
        }


    }


}
