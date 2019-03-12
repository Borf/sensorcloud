using System;
using System.Collections.Generic;
using System.Text;
using SensorCloud.modules;

namespace SensorCloud
{
	public abstract class Module
	{
		public abstract void Start();

		//TODO: make this a bit better, don't just jam it all in here
		private static ConsoleColor[] usableColors = {
			ConsoleColor.Red,
			ConsoleColor.Green,
			ConsoleColor.Magenta,
			ConsoleColor.White,
			ConsoleColor.Yellow };
		private static int lastColor = -1;
		private ConsoleColor color = ConsoleColor.Black;


		public string moduleName { get; private set; }


		public Module()
		{
			ModuleManager.Add(this);

			moduleName = this.GetType().Name.ToUpper();
			if (moduleName.Contains("MODULE"))
				moduleName = moduleName.Substring(0, moduleName.IndexOf("MODULE"));
			lastColor = (lastColor + 1) % usableColors.Length;
			color = usableColors[lastColor];
		}

		protected T GetModule<T>() where T : Module
		{
			return ModuleManager.GetModule<T>();
		}


		public virtual void HandleCommand(string command)
		{
			Console.WriteLine("Unhandled command");
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
