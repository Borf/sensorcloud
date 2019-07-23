using System.Threading.Tasks;

namespace SensorCloud.rules
{
    internal class TelegramMessageComponent : Component
    {
        services.telegram.Service telegramService;

        public TelegramMessageComponent(Service service) : base("Send Telegram Message")
        {
            addInput("text", new TextSocket());
            addInput("trigger", new ActionSocket());
            telegramService = service.GetService<services.telegram.Service>();
        }

        public override async Task<bool> OnTrigger(Node node)
        {
            string input = node.getInputValue("text") as string;
            await telegramService.SendMessageAsync(input);
            return true;
        }
    }
}