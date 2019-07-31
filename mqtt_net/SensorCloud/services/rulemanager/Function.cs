using Newtonsoft.Json;
using SensorCloud.rules;
using System;
using System.Collections.Generic;

namespace SensorCloud.services.rulemanager
{
    public partial class Service
    {
        public class Function
        {
            public string Module { get; set; }
            public string FunctionName { get; set; }
            public List<Tuple<string, Socket>> Parameters { get; set; }
            [JsonIgnore]
            public Action<Dictionary<string, object>> Callback { get; set; }
        }


    }


}
