using System.ComponentModel.DataAnnotations.Schema;

namespace SensorCloud.datamodel
{
    [Table("sensortypes")]
	public class Sensortype
	{
		public int id { get; set; }
		public string name { get; set; }
		public int isSensor { get; set; }
		
	}
}
