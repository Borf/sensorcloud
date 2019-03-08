using System;

namespace SensorCloud.datamodel
{
	public class SensorData
	{
		public int id { get; set; }
		public DateTime stamp { get; set; }
		public int nodeid { get; set; }
		public string type { get; set; }
		public double value { get; set; }
	}
}
