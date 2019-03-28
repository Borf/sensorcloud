using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SensorCloud.services.dash
{
    public partial class Service
    {
        private async Task handleHttpAsync(DashboardItem item)
        {
            var parameters = item.parameter.Split("|");
            try
            {
                WebRequest request = WebRequest.Create(parameters[0]);
                WebResponse response = await request.GetResponseAsync();
                item.value = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                if (item.value.Contains(parameters[1]))
                    item.value = "ok";
                else
                    item.value = "error";

            }
            catch (WebException e)
            {
                if (parameters[1] == "auth" && e.Message.Contains("401"))
                    item.value = "ok";
                else
                    item.value = "error";
            }

        }
    }
}
