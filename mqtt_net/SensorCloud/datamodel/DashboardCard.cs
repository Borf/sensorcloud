using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SensorCloud.datamodel
{
    [Table("dashboard_cards")]
    public class DashboardCard
    {
		public int id { get; set; }
        public string title { get; set; }
		public int order { get; set; }
        public int columns { get; set; }

        public virtual ICollection<DashboardItem> items { get; set; }

    }
}
