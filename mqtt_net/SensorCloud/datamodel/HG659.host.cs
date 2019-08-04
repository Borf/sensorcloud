using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensorCloud.datamodel
{
    [Table("HG659.hosts")]
    public class HG659_host
	{
        public string mac { get; set; }
        public string ip { get; set; }
        public string hostname { get; set; }
    }
}
