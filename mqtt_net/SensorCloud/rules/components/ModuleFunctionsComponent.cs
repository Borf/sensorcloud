using System.Threading.Tasks;

namespace SensorCloud.rules
{
    internal class ModuleFunctionsComponent : Component
    {
        services.rulemanager.Service rulesManager;
        public ModuleFunctionsComponent(Service service) : base("Module functions")
        {
            rulesManager = service.GetService<services.rulemanager.Service>();

            addInput("trigger", new ActionSocket());
            addInput("module", new TextSocket());
            addInput("function", new TextSocket());
        }

        public override async Task<bool> OnTrigger(Node node)
        {
            rulesManager.functions.Find(r => 
                r.Module == node.data["module"].ToObject<string>() &&
                r.FunctionName == node.data["function"].ToObject<string>()).Callback(node.inputValues);
            return false;
        }
    }
}