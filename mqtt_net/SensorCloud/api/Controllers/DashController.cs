using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DashController : Controller
    {
        SensorCloudContext db;

        public DashController(SensorCloudContext db)
        {
            this.db = db;
        }

        [HttpGet("cards")]
        [AllowAnonymous]
        public IEnumerable<DashboardCard> Cards()
        {
            return db.dashboardCards.OrderBy(c => c.order);
        }

        [HttpGet("card/{id}")]
        public IEnumerable<DashboardItem> Cards(int id)
        {
            return db.dashboardItems.Where(i => i.cardid == id);
        }
    }
}
