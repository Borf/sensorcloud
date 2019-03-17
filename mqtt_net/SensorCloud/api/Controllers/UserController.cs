using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : Controller
    {


        [HttpPost("login")]
        [AllowAnonymous]
        public object Login([FromBody]JObject value)
        {
            return new { auth = true };
        }
    }
}
