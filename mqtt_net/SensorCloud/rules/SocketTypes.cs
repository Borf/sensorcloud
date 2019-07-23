using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules
{
    public class Socket
    {
        public string name { get; set; }
        public Socket(string name) { this.name = name; }
    }

    public class ActionSocket : Socket
    {
        public ActionSocket() : base("Action") { }
    }
    public class TextSocket : Socket
    {
        public TextSocket() : base("Text") { }
    }
    public class NumberSocket : Socket
    {
        public NumberSocket() : base("Number") { }
    }
}
