using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.datamodel
{
    public class Config
    {
        public Mysql mysql { get; set; }
    }

    public class Mysql
    {
        public string host { get; set; }
        public int port { get; set; }
        public string user { get; set; }
        public string pass { get; set; }
        public string daba { get; set; }

        public string configstring {
            get {
                return $"server={host};database={daba};user={user};password={pass}";
            }
        }
    }
}
