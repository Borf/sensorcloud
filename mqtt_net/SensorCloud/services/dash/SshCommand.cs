using Newtonsoft.Json.Linq;
using Renci.SshNet;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorCloud.services.dash
{
    public partial class Service
    {
        public void handleSshCommand(DashboardItem item)
        {
            try
            {
                var options = item.parameter.Split("|", 2);
                var host = config.hosts[options[0]];
                using (var client = new SshClient(host.Host, host.User, Encoding.UTF8.GetString(Convert.FromBase64String(host.Pass))))
                {
                    client.Connect();
                    string ret = client.RunCommand(options[1]).Execute().Trim();
                    item.value = ret;
                    client.Disconnect();
                }
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

