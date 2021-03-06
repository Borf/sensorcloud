using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P1Meter
{
    public class DataPacket
    {
        public bool IsValid {
            get {
                return PowerConsumptionTariff1 != 0 && PowerConsumptionTariff2 != 0 && GasUsage != 0;
            }
        }

        [Marker("1-0:1.8.1")]
        public decimal PowerConsumptionTariff1 { get; set; } = 0M;

        [Marker("1-0:1.8.2")]
        public decimal PowerConsumptionTariff2 { get; set; } = 0M;

        [Marker("0-1:24.2.1")]
        public decimal GasUsage { get; set; } = 0M;

		public override string ToString()
		{
			return $"P1 packet: P1: {PowerConsumptionTariff1}, P2 {PowerConsumptionTariff2}, Gas {GasUsage}";
		}

        static Dictionary<string, PropertyInfo> attributes = new Dictionary<string, PropertyInfo>();
        static DataPacket()
        {
            var properties = typeof(DataPacket).GetProperties();
            foreach (var property in properties)
            {
                MarkerAttribute attribute = property.GetCustomAttributes(typeof(MarkerAttribute), false).Cast<MarkerAttribute>().FirstOrDefault();
                if (attribute != null)
                    attributes[attribute.marker] = property;
            }
        }

        internal void ParseLine(string line)
        {
            if (line.Contains("("))
            {
                string header = line.Substring(0, line.IndexOf("("));
                if (attributes.ContainsKey(header))
                {
                    line = line.Substring(header.Length);
                    line = line.Replace("(", "");
                    line = line.Replace(")", ",");
                    string[] _params = line.Split(",");
                    if (_params[0].Contains("*"))
                        _params[0] = _params[0].Substring(0, _params[0].IndexOf("*"));
                    try
                    {
                        decimal d = decimal.Parse(_params[0]);
                        attributes[header].SetValue(this, d);
                    }catch(System.FormatException e)
                    {
                        Console.WriteLine(line);
                        Console.WriteLine(e);
                    }
                }
            }
        }
    }
}
