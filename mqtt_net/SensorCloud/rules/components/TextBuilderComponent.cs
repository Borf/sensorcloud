using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules.components
{
    public class TextBuilderComponent : Component
    {
        public TextBuilderComponent() : base("Text Builder")
        {
            addOutput("text", new TextSocket());
            //addInput("val1", new TextSocket());
            //addInput("val2", new TextSocket());
        }

        public override void SetOutputs(Node node)
        {
            string output = "";
            for(int i = 1; i <= node.data.Count; i++)
                output += (string)node.getInputValue("val" + i);
            
            node.outputValues["text"] = output;
        }
    }
}
