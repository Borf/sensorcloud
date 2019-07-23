namespace SensorCloud.rules
{
    internal class TelegramMessageReceivedComponent : Component
    {
        public TelegramMessageReceivedComponent() : base("Receive Telegram Message")
        {
            addOutput("text", new TextSocket());
            addOutput("trigger", new ActionSocket());
        }
    }
}