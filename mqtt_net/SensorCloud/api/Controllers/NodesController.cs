﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SensorCloud.datamodel;


namespace SensorCloud.api.Controllers
{
	[Route("[controller]")]
    [ApiController]
    public class NodesController : Controller
	{
		SensorCloudContext db;

        /*public NodesController()
		{
			db = ModuleManager.GetModule<DbModule>().context;
		}

        [HttpGet]
		public IEnumerable<Node> Get()
		{
			return db.nodes;
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
		}*/
	}
}
