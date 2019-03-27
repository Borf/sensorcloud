using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SensorCloud.services
{
    interface IConfigServiceBase
    {
        void init(IServiceCollection services, IConfiguration configuration);
    }
    class ConfigService<Service, Config> : IConfigServiceBase where Service : class, IHostedService 
                                                                where Config : class, new()
    {
        private string configSectionName;

        public ConfigService(string configSectionName)
        {
            this.configSectionName = configSectionName;
        }

        public void init(IServiceCollection services, IConfiguration configuration)
        {
            bool enabled = true;
            if (configuration[configSectionName + ":enabled"] != null &&
                configuration[configSectionName + ":enabled"] == "False")
                enabled = false;
            if (enabled)
            {
                System.Console.WriteLine("APP\t\tLoading service " + configSectionName);
                Config config = new Config();
                configuration.GetSection(configSectionName).Bind(config);
                services.AddSingleton<Config>(config);

                services.AddSingleton<IHostedService, Service>();
                services.AddSingleton<Service>(sp => sp.GetServices<IHostedService>().ToList().Find(x => x.GetType() == typeof(Service)) as Service);
            }
            else
                System.Console.WriteLine("APP\t\tCould not find config for service " + configSectionName);
        }
    }
}
