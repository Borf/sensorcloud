using System.Threading.Tasks;

namespace SensorCloud.rules
{
    internal class MqttSubscribeComponent : Component
    {
        public MqttSubscribeComponent() : base("Mqtt Subscribe")
        {
            addInput("topic", new TextSocket());
            addOutput("payload", new TextSocket());
            addOutput("trigger", new ActionSocket());
        }

        public override async Task<bool> OnTrigger(Node node)
        {
            if (node.data["in"]["topic"].ToObject<string>() == (string)node.getInputValue("topic"))
                return true;
            return false;
        }
    }
}