using System.Collections.Generic;

namespace SensorCloud.datamodel
{
	public class Room
	{
		public int id { get; set; }
		public string name { get; set; }
		public string topic { get; set; }
		public virtual ICollection<Node> nodes { get; set; }
	}
}
