using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private services.status.Service status;

        public StatusController(services.status.Service status)
        {
            this.status = status;
        }

        [HttpGet]
        public object Get()
        {
            return status.status;
        }



    }
}