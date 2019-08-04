using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SensorCloud
{
	public abstract class Service : BackgroundService
	{
        private ConsoleColor color = ConsoleColor.Black;
        protected IServiceProvider services { private set; get; }
        public string moduleName { get; private set; }

        public string moduleNameFirstCap {  get { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(moduleName.ToLower()); }  }

        public Service(IServiceProvider services)
        {
            this.services = services;
            moduleName = this.GetType().Name.ToUpper();
            if (moduleName == "SERVICE")
            {
                moduleName = this.GetType().Namespace.ToUpper();
                moduleName = moduleName.Substring(moduleName.LastIndexOf(".")+1);
            }
            else if (moduleName.Contains("SERVICE"))
                moduleName = moduleName.Substring(0, moduleName.IndexOf("SERVICE"));
            lastColor = (lastColor + 1) % usableColors.Length;
            color = usableColors[lastColor];
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await base.StartAsync(cancellationToken);
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
            var telegram = GetService<services.telegram.Service>();
            if (telegram != null)
                InstallTelegramHandlers(telegram);

            var mqtt = GetService<services.mqtt.Service>();
            if (mqtt != null)
                InstallMqttHandlers(mqtt);
        }


        // overrides
        public virtual void HandleCommand(string command)
        {
            Console.WriteLine("Unhandled command");
        }

        public virtual void InstallTelegramHandlers(services.telegram.Service telegram)
        {
        }

        public virtual void InstallMqttHandlers(services.mqtt.Service mqtt)
        {
        }


        //TODO: make this a bit better, don't just jam it all in here
        private static ConsoleColor[] usableColors = {
			ConsoleColor.Red,
			ConsoleColor.Green,
			ConsoleColor.Magenta,
			ConsoleColor.White,
			ConsoleColor.Yellow };
		private static int lastColor = -1;


        public T GetService<T>()
        {
            using (var scope = services.CreateScope())
            {
                return scope.ServiceProvider.GetService<T>();

            }
        }



		public void Log(string msg)
		{
			Console.ForegroundColor = color;
			Console.Write($"{moduleName}\t");
			if (moduleName.Length < 8)
				Console.Write("\t");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(msg);

		}
	}
}
