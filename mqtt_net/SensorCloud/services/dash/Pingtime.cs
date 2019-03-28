using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.dash
{
    public partial class Service
    {
        private void handlePingTime(DashboardItem item)
        {
            AutoResetEvent waiter = new AutoResetEvent(false);
            var ping = new System.Net.NetworkInformation.Ping();
            ping.PingCompleted += (s, e) =>
            {
                if (e.Reply.Status == IPStatus.Success)
                    item.value = e.Reply.RoundtripTime + "";
                else
                    item.value = "error";
                waiter.Set();
            };
            ping.SendAsync(item.parameter, 100, null);
            waiter.WaitOne();
        }
    }
}
