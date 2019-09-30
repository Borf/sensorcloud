using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.status
{
    public class Service : SensorCloud.Service
    {
        public Service(IServiceProvider services) : base(services)
        {
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        new public Dictionary<string, string> status = new Dictionary<string, string>();

        public string this[string key] {
            get { return status[key]; }
            set { status[key] = DateTime.Now.ToString() + " - " + value; }
        }
    }
}
