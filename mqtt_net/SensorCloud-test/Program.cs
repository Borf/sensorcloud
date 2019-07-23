using Microsoft.Extensions.Configuration;
using SensorCloud.datamodel;
using SensorCloud.rules;
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

            //new P1MeterTest().TestHistory();

            Engine.init(null);
            var engine = new Engine(@"{""id"":""demo@0.1.0"",""nodes"":{""1"":{""id"":1,""data"":{},""inputs"":{},""outputs"":{""text"":{""connections"":[]},""trigger"":{""connections"":[{""node"":2,""input"":""trigger"",""data"":{}}]}},""position"":[187.1567602452404,403.0673120361407],""name"":""Receive Telegram Message""},""2"":{""id"":2,""data"":{},""inputs"":{""text"":{""connections"":[{""node"":3,""output"":""text"",""data"":{}}]},""trigger"":{""connections"":[{""node"":1,""output"":""trigger"",""data"":{}}]}},""outputs"":{},""position"":[957.76488625829,252.0673892890252],""name"":""Send Telegram Message""},""3"":{""id"":3,""data"":{""text"":""Hoi""},""inputs"":{},""outputs"":{""text"":{""connections"":[{""node"":2,""input"":""text"",""data"":{}}]}},""position"":[447,126],""name"":""Text""}}}");
            engine.trigger("Receive Telegram Message", new Dictionary<string, object>() { { "name", "Hello" } });
        }
    }
}
