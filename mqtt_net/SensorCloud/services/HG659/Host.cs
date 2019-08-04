using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.HG659
{
    public class Host
    {
        public bool Active { get; set; }
        public string QosclassID { get; set; }
        public int DeviceMaxDownLoadRate { get; set; }
        public string HostName { get; set; }
        public bool Active46 { get; set; }
        public int LeaseTime { get; set; }
        public string ID { get; set; }
        public List<object> Ipv6Addrs { get; set; }
        public int ClassQueue { get; set; }
        public string Layer2Interface { get; set; }
        public string ActualName { get; set; }
        public string IPAddress { get; set; }
        public string PolicerID { get; set; }
        public string domain { get; set; }
        public bool DeviceDownRateEnable { get; set; }
        public string MACAddress { get; set; }
        public bool ParentControlEnable { get; set; }
        public string MacFilterID { get; set; }
        public string AddressSource { get; set; }
        public bool V6Active { get; set; }
        public string IconType { get; set; }
        public string IPv6Address { get; set; }
        public string VendorClassID { get; set; }
    }
}
