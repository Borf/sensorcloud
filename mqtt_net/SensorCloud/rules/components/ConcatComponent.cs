using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules.components
{
    public class ConcatComponent : Component
    {
        public ConcatComponent() : base("Concatenate")
        {
            addOutput("text", new TextSocket());
            addInput("val1", new TextSocket());
            addInput("val2", new TextSocket());
        }


        //spotnet.title[0].cookie
        public override void SetOutputs(Node node)
        {
            string val1 = (string)node.getInputValue("val1");
            string val2 = (string)node.getInputValue("val2");
            node.outputValues["text"] = val1+val2;
        }
    }
}
