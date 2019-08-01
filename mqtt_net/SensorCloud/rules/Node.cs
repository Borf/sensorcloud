using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules
{
    public class Connection
    {
        public int node { get; set; }
        public string connector { get; set; }
    }
    public class Node
    {
        public int id { get; set; }
        public Component component;
        public Dictionary<string, Connection> inputs = new Dictionary<string, Connection>();
        public Dictionary<string, List<Connection>> outputs = new Dictionary<string, List<Connection>>();
        public Dictionary<string, object> outputValues = new Dictionary<string, object>();
        public Dictionary<string, object> inputValues = new Dictionary<string, object>();
        public JObject data;

        public Node(Component component)
        {
            this.component = component;
        }

        public void SetParameters(Dictionary<string, object> parameters)
        {
            foreach (var kp in outputs)
                if (parameters.ContainsKey(kp.Key))
                    outputValues[kp.Key] = parameters[kp.Key];
            data["in"] = JObject.FromObject(parameters);
        }

        public void SetInputs(Engine engine)
        {
            foreach (var kp in inputs)
            {
                if (component.inputs.ContainsKey(kp.Key) && component.inputs[kp.Key] is ActionSocket)
                    continue;
                engine.nodes[kp.Value.node].SetInputs(engine);
                engine.nodes[kp.Value.node].SetOutputs();
                inputValues[kp.Key] = engine.nodes[kp.Value.node].outputValues[kp.Value.connector];
            }
        }

        public void SetOutputs()
        {
            component.SetOutputs(this);
        }

        public void trigger(Engine engine)
        {
            //first update all inputs
            SetInputs(engine);

            if (!component.trigger(this).Result)
                return;
            //if this node has any connected triggerable objects, trigger them
            foreach (var kp in outputs)
                if(component.outputs.ContainsKey(kp.Key) && component.outputs[kp.Key].name == "Action")
                    foreach (var connection in kp.Value)
                        engine.nodes[connection.node].trigger(engine);
        }

        internal object getInputValue(string name)
        {
            return inputValues[name];
        }

    }


}
