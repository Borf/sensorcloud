using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SensorCloud_test
{
    class Program
    {
        static void Main(string[] args)
        {
            //new UnitTest1().TestMethod1();
            //System.Diagnostics.Process.Start(@"c:\Program Files\IrfanView\i_view64.exe", "test.png");

            new P1MeterTest().TestHistory();

        }
    }
}
