using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules.components
{
    public class LogComponent : Component
    {
        public LogComponent() : base("Log")
        {
            addInput("text", new TextSocket());
            addInput("trigger", new ActionSocket());
        }
        public override Task<bool> OnTrigger(Node node)
        {
            Console.WriteLine(node.inputValues["data"]);
            return Task.FromResult(true);
        }
    }
}