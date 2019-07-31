using System.Threading.Tasks;

namespace SensorCloud.rules
{
    internal class IfComponent : Component
    {
        public IfComponent() : base("If")
        {
            addInput("trigger", new ActionSocket());
            addOutput("trigger", new ActionSocket());
            addInput("val1", new TextSocket());
            addInput("comparator", new TextSocket());
            addInput("val2", new TextSocket());
        }

        public override async Task<bool> OnTrigger(Node node)
        {
            if ((string)node.getInputValue("val1") == (string)node.getInputValue("val2"))
                return true;
            return false;
        }
    }
}