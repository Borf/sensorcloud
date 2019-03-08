using System;

namespace SensorCloud.datamodel
{
	public class Ping
	{
		public int id { get; set; }
		public DateTime stamp { get; set; }
		public int nodeid { get; set; }
		public string ip { get; set; }
		public int heapspace { get; set; }
		public int rssi { get; set; }
	}
}
