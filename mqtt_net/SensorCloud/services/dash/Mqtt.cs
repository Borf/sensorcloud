using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.dash
{
    public partial class Service
    {
        private void StartMqtt(DashboardItem item)
        {
            var options = item.parameter.Split("|");
            mqtt.storeLastValue(options[0]);
        }

        private void HandleMqtt(DashboardItem item)
        {
            var options = item.parameter.Split("|");
            mqtt.storeLastValue(options[0]);
            if (!mqtt.lastValues.ContainsKey(options[0]))
                item.value = "error";
            else
            {
                string value = mqtt.lastValues[options[0]];
                for (int i = 1; i < options.Length; i++)
                {
                    var v = options[i].Split("=");
                    if (value == v[0])
                        item.value = v[1];
                }
            }
        }
    }
}
