using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SensorCloud.datamodel;


namespace SensorCloud.api.Controllers
{
	[Route("[controller]")]
    [ApiController]
    public class NodesController : Controller
	{
		SensorCloudContext db;

        public NodesController(SensorCloudContext db)
		{
            this.db = db;
		}

        [HttpGet]
		public object Get()
		{
            return db.nodes
                .Include(n => n.Room)
                .Include(n => n.sensors)
                    .ThenInclude(s => s.Type)
                .AsEnumerable().Select(n =>
            {
                var ping = db.pings.OrderByDescending(p => p.id).FirstOrDefault(p => p.nodeid == n.id);
                return new
                {
                    id = n.id,
                    hwid = n.hwid,
                    name = n.name,
                    room = n.Room.name,
                    ip = ping?.ip,
                    rssi = ping?.rssi,
                    heapspace = ping?.heapspace,
                    stamp = ping?.stamp,
                    lastsensordata = "",
                    sensorcount = n.sensors.Count(s => s.Type.isSensor == 1),
                    actcount = n.sensors.Count(s => s.Type.isSensor == 0)
                };
            });
		}

		// GET api/<controller>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// POST api/<controller>
		[HttpPost]
		public void Post([FromBody]string value)
		{
		}

		// PUT api/<controller>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody]string value)
		{
		}

		// DELETE api/<controller>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
