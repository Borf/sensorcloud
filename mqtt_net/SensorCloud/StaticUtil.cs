using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SensorCloud
{
    public static class StaticUtil
    {
        public static string ToPubDate(this DateTime date)
        {
            return date.ToString("ddd, d MMMM yyyy hh:mm:ss +0200");
        }
    }
}
