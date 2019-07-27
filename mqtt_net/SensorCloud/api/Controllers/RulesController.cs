using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SensorCloud.services.rulemanager;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RulesController : ControllerBase
    {
        private SensorCloudContext db;
        private services.rulemanager.Service rulesManager;

        public RulesController(SensorCloudContext db, services.rulemanager.Service rulesManager)
        {
            this.db = db;
            this.rulesManager = rulesManager;
        }

        [HttpGet(":{name}")]
        public object Get(string name)
        {
            return db.rules.FirstOrDefault(s => s.name == name);
        }

        [HttpGet("")]
        public object Get()
        {
            return db.rules.Select(s => new { id = s.id, name = s.name, enabled = s.enabled });
        }

        [HttpPost("enable:{name}")]
        public async Task<object> Enable(string name)
        {
            var rule = db.rules.FirstOrDefault(r => r.name == name);
            if (rule == null)
                return "error";
            rule.enabled = 1;
            await db.SaveChangesAsync();
            return "ok";
        }

        [HttpPost("disable:{name}")]
        public async Task<object> Disable(string name)
        {
            var rule = db.rules.FirstOrDefault(r => r.name == name);
            if (rule == null)
                return "error";
            rule.enabled = 0;
            await db.SaveChangesAsync();
            return "ok";
        }

        [HttpPost("update/:{name}")]
        public async Task<object> update(string name, [FromBody]JObject value)
        {
            var rule = db.rules.FirstOrDefault(r => r.name == name);
            if (rule == null)
            {
                rule = new datamodel.Rule() { name = name, enabled = 1 };
                db.rules.Add(rule);
            }
            rule.data = value["data"].ToObject<string>();
            await db.SaveChangesAsync();

            rulesManager.ReloadRule(rule.id);

            return "ok";
        }

    }
}