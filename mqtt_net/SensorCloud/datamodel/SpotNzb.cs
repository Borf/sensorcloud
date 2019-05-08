using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.datamodel
{
    [Table("spotnzbs")]
    public class SpotNzb
    {
        [Column("article")]
        public long article { get; set; }

        [Column("articleid")]
        public string articleid { get; set; }
        [Column("segment")]
        public string segment { get; set; }


        [ForeignKey("article")]
        public Spot spot { get; set; }
    }
}
