using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SensorCloud.api;
using SensorCloud.services;
using SensorCloud.services.mtt;
using SensorCloud.services.onkyo;

namespace api
{
    public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
        public static Microsoft.AspNetCore.Hosting.IHostingEnvironment env { get; private set; }
        public static bool IsDevelopment { get { return env.IsDevelopment(); } }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
		{
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,
                builder =>
                {
		            builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
                });
			});
            services.AddMvc(options =>
			{
                options.Filters.Add(new ApiFilter());
				options.OutputFormatters.Clear();
				options.OutputFormatters.Add(new JsonOutputFormatter(new JsonSerializerSettings()
				{
					ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				}, ArrayPool<char>.Shared));
			}).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IConfiguration>(Configuration);


            foreach (var s in ConfigServices.services)
                s.init(services, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
            Startup.env = env;
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = ctx =>
                {
                    //no static files on api...
                    if (ctx.Context.Request.Host.Host.Contains("api"))
                    {
                        ctx.Context.Response.Headers.Clear();
                        ctx.Context.Response.StatusCode = 404;
                        ctx.Context.Response.Body = new MemoryStream();
                    }
                }
            });
			app.UseMvc();
		}
	}
}
