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
        private async Task handleGetAsync(DashboardItem item)
        {
            try
            {
                WebRequest request = WebRequest.Create(item.parameter);
                WebResponse response = await request.GetResponseAsync();
                item.value = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
            }
            catch (WebException)
            {
                item.value = "error";
            }
        }
    }
}
