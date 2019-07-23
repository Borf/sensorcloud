using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules.components
{
    public class TextComponent : Component
    {
        public TextComponent() : base("Text")
        {
            addOutput("text", new TextSocket());
        }

        public override void SetOutputs(Node node)
        {
            node.outputValues["text"] = node.data["text"].ToObject<string>();
        }
    }
}
