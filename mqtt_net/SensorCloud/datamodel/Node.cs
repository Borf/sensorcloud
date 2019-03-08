using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SensorCloud.datamodel
{
	public class Node
	{
		public int id { get; set; }
		public string hwid { get; set; }
		public string name { get; set; }
		public string topic { get; set; }

		[Column("room")]
		public int roomId { get; set; }
		[ForeignKey("roomId")]
		public Room Room { get; set; }

		public string config { get; set; }
		public virtual ICollection<Sensor> sensors { get; set; }

		public override string ToString()
		{
			return $"Node({id}, {hwid}, {name}, {Room?.name}, {sensors?.Count} sensors";
		}
	}
}
