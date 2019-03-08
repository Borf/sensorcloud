using System.ComponentModel.DataAnnotations.Schema;

namespace SensorCloud.datamodel
{
	public class Sensor
	{
		public int id { get; set; }
		public int nodeid { get; set; }

		[ForeignKey("nodeid")]
		public Node node { get; set; }
		public int type { get; set; }
		public string config { get; set; }
	}
}
