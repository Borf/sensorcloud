using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensorCloud.datamodel
{
	[Table("dashboard_items")]
	public class DashboardItem
	{
		public int id { get; set; }
		public int cardid { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public string parameter { get; set; }
		public DateTime time { get; set; }
		public string value { get; set; }

        [ForeignKey("cardid")]
        public DashboardCard card { get; set; }
    }
}
