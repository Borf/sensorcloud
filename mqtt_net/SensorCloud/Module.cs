using System;
using System.Collections.Generic;
using System.Text;
using SensorCloud.modules;

namespace SensorCloud
{
	public abstract class Module
	{
        public abstract void Start();


        static List<Module> modules = new List<Module>();

		public Module()
		{
			modules.Add(this);
		}
        protected static void removeModule(Module module)
        {
            modules.Remove(module);
        }

		public static T GetModule<T>() where T : Module
		{
			foreach (Module m in modules)
			{
				if (m is T)
					return (T)m;
			}
			return null;
		}

		public static IEnumerable<T> GetModules<T>() where T : Module
		{
			List<T> ret = new List<T>();
			foreach (Module m in modules)
			{
				if (m is T)
					ret.Add((T)m);
			}
			return ret;
		}

		public static void StartAll()
		{
			foreach (Module m in modules)
				m.Start();
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
			if(color == ConsoleColor.Black) //uninitialized
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
