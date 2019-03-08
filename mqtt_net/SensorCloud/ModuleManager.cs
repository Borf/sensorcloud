using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud
{
	public class ModuleManager
	{
		static List<Module> modules = new List<Module>();
		public static void Add(Module module)
		{
			modules.Add(module);
		}
		public static void Remove(Module module)
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
	}
}
