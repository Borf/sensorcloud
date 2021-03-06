﻿using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using SensorCloud.rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.rulemanager
{
    public partial class Service : SensorCloud.Service
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

        public void ReloadRule(int id)
        {
            Log($"Reloading rule {id}");
            using (SensorCloudContext db = new SensorCloudContext(configuration))
            {
                rules.RemoveAll(r => r.id == id);
                var rule = db.rules.FirstOrDefault(r => r.id == id && r.enabled == 1);
                if(rule != null)
                    rules.Add(new Rule()
                    {
                        engine = new Engine(rule.data),
                        name = rule.name,
                        id = rule.id
                    });
            }
            UpdateLists();
        }

        public void UpdateLists()
        {
            cache.Clear();
        }

        public void trigger(string triggerObject, Dictionary<string, object> parameters)
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

        public void triggerModuleCommand(string module, string triggerName, Dictionary<string, object> parameters)
        {
            if (rules == null)
                return;
            if (!cache.ContainsKey("Module triggers"))
                cache["Module triggers"] = rules.Where(r => r.engine.ContainsTrigger("Module triggers")).ToList();

            cache["Module triggers"].Where(e =>
            {
                var n = e.engine.FindNode("Module triggers");
                return n.data["module"].ToObject<string>() == module && n.data["function"].ToObject<string>() == triggerName;
                }).ToList().ForEach(
                rule => rule.engine.trigger("Module triggers",
                parameters));
        }

        public List<Function> functions = new List<Function>();
        public List<Trigger> triggers= new List<Trigger>();

        public void AddFunction(Function function)
        {
            this.functions.Add(function);
        }

        public void RemoveFunction(Function function)
        {
            this.functions.Remove(function);
        }
        public void AddTrigger(Trigger trigger)
        {
            this.triggers.Add(trigger);
        }

        public void RemoveTrigger(Trigger trigger)
        {
            this.triggers.Remove(trigger);
        }


    }


}
