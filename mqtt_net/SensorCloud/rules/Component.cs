using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules
{
    public class Component
    {
        public string name { get; set; }
        public Dictionary<string, Socket> inputs = new Dictionary<string, Socket>();
        public Dictionary<string, Socket> outputs = new Dictionary<string, Socket>();

        public Component(string name)
        {
            this.name = name;
        }

        public void addInput(string name, Socket socket)
        {
            inputs[name] = socket;
        }
        public void addOutput(string name, Socket socket)
        {
            outputs[name] = socket;
        }

        public async Task<bool> trigger(Node node)
        {
            //fix inputs
            return await OnTrigger(node);
        }

        public virtual Task<bool> OnTrigger(Node node)
        { return Task.FromResult(true); }

        public virtual void SetOutputs(Node node)
        { }
    }
}
