using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using SensorCloud.services.rulemanager;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private SensorCloudContext db;

        public RoomController(SensorCloudContext db, services.rulemanager.Service rulesManager)
        {
            this.db = db;
        }

        [HttpGet("")]
        public object Get()
        {
            return db.rooms.Include(room => room.nodes);
        }


    }
}