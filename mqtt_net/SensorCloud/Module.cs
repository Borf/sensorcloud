using System;
using System.Collections.Generic;
using System.Text;
using SensorCloud.modules;

namespace SensorCloud
{
	public abstract class Module
	{
		public abstract void Start();


		public Module()
		{
			ModuleManager.Add(this);
		}

		protected T GetModule<T>() where T : Module
		{
			return ModuleManager.GetModule<T>();
		}



		//TODO: make this a bit better, don't just jam it all in here
		private static ConsoleColor[] usableColors = {
			ConsoleColor.Red,
			ConsoleColor.Green,
			ConsoleColor.Magenta,
			ConsoleColor.White,
			ConsoleColor.Yellow };
		private static int lastColor = -1;
		private ConsoleColor color = ConsoleColor.Black;
		private String moduleName;
		public void Log(string msg)
		{
			if (color == ConsoleColor.Black) //uninitialized
			{
				moduleName = this.GetType().Name.ToUpper();
				if (moduleName.Contains("MODULE"))
					moduleName = moduleName.Substring(0, moduleName.IndexOf("MODULE"));
				lastColor = (lastColor + 1) % usableColors.Length;
				color = usableColors[lastColor];
			}

			Console.ForegroundColor = color;
			Console.Write($"{moduleName}\t");
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.WriteLine(msg);

		}
	}
}
