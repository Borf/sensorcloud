using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensorCloud.datamodel;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SensorDataController : ControllerBase
    {
        private SensorCloudContext db;

        public SensorDataController(SensorCloudContext db)
        {
            this.db = db;
        }

        [HttpGet(":{type}")]
        public object Get(string type)
        {
            var a = db.sensors.Select(
                s => new
                {
                    node = s.nodeid,
                   // recent = s.measurements.Where(m => m.type == type).Take(100).Select(e => new { date = e.stamp, value = e.value })
                });
 


            return a;
        }

    }
}