using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.api
{
    public class DomainConstraint : IRouteConstraint
    {
        private readonly string[] domains;

        public DomainConstraint(params string[] domains)
        {
            this.domains = domains ?? throw new ArgumentNullException(nameof(domains));
        }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string domain =
#if DEBUG
                // A domain specified as a query parameter takes precedence 
                // over the hostname (in debug compile only).
                // This allows for testing without configuring IIS with a 
                // static IP or editing the local hosts file.
                httpContext.Request.Query["domain"];
#else
                null;
#endif
            if (string.IsNullOrEmpty(domain))
                domain = httpContext.Request.Host.Host;

            Console.WriteLine("MATCHING!!!!!!!!!!!!");
            Console.WriteLine("MATCHING!!!!!!!!!!!!");
            Console.WriteLine("MATCHING!!!!!!!!!!!!");
            Console.WriteLine("MATCHING!!!!!!!!!!!!");

            return domains.Contains(domain);
        }
    }
}
