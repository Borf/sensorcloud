﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SensorCloud.api
{
    public class ApiFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            
        }

        string[] hosts = { "localhost", "api.sensorcloud.borf.nl", "api.sensorcloud.borf.info", "10.10.0.198" };
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if(!hosts.Contains(context.HttpContext.Request.Host.Host))
			{
				Console.WriteLine("Blocking " + context.HttpContext.Request.Host.Host);
                context.Result = new NotFoundResult();
            }
        }
    }
}
