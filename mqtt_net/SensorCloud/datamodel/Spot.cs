using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.datamodel
{
    public class Spot
    {
        public long article { get; set; }
        [Column("articleid")]
        public string articleid { get; set; }
        public DateTime posted { get; set; }
        public string title { get; set; }
        public string cat { get; set; }
        public string subcat { get; set; }
        public long size { get; set; }
        public string desc { get; set; }

        public virtual ICollection<SpotNzb> nzbs { get; set; }

    }
}
