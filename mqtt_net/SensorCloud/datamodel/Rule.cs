using System;

namespace SensorCloud.datamodel
{
	public class Rule
	{
		public int id { get; set; }
		public string name { get; set; }
		public int enabled { get; set; }
		public string data { get; set; }
	}
}
