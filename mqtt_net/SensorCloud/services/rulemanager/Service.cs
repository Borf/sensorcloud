using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using SensorCloud.rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.rulemanager
{
    public class Service : SensorCloud.Service
    {
        private IConfiguration configuration;

        private List<Rule> rules = null;
        Dictionary<string, List<Rule>> cache = new Dictionary<string, List<Rule>>();

        public Service(IServiceProvider services, IConfiguration configuration) : base(services)
        {
            this.configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Engine.init(this);
            using (SensorCloudContext db = new SensorCloudContext(configuration))
            {
                rules = new List<Rule>();
                db.rules.Where(r => r.enabled == 1).ToList().ForEach(r =>
                    rules.Add(new Rule()
                    {
                        engine = new Engine(r.data),
                        name = r.name,
                        id = r.id
                    }));
            }
            UpdateLists();
            return Task.CompletedTask;
        }

        public void UpdateLists()
        {
            cache.Clear();
        }

        public void Trigger(string triggerObject, Dictionary<string, object> parameters)
        {
            if (rules == null)
                return;
            if(!cache.ContainsKey(triggerObject))
            {
                cache[triggerObject] = rules.Where(r => r.engine.ContainsTrigger(triggerObject)).ToList();
            }
            cache[triggerObject].ForEach(
                rule => rule.engine.trigger(triggerObject, 
                parameters));

        }
    }


}
