using System.Threading.Tasks;

namespace SensorCloud.rules
{
    internal class ModuleTriggersComponent : Component
    {
        services.rulemanager.Service rulesManager;
        public ModuleTriggersComponent(Service service) : base("Module triggers")
        {
            rulesManager = service.GetService<services.rulemanager.Service>();

            addOutput("trigger", new ActionSocket());
            addInput("module", new TextSocket());
            addInput("function", new TextSocket());
        }

        public override async Task<bool> OnTrigger(Node node)
        {
            return rulesManager.triggers.Find(r =>
                r.Module == node.data["module"].ToObject<string>() &&
                r.TriggerName == node.data["function"].ToObject<string>()).Callback(node);
        }
    }
}