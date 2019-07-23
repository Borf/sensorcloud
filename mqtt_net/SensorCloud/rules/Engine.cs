using Newtonsoft.Json.Linq;
using SensorCloud.rules.components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules
{
    public class Engine
    {
        public Dictionary<int, Node> nodes = new Dictionary<int, Node>();

        public Engine(string data)
        {
            try
            {
                var d = JObject.Parse(data);
                foreach (JProperty e in d["nodes"])
                {
                    var n = e.Value;
                    Component component = components.FirstOrDefault(c => c.name == n["name"].ToObject<string>());
                    if(component == null)
                        Console.WriteLine($"Could not find component {n["name"]}");
                    Node node = new Node(component);
                    node.id = (int)n["id"];
                    foreach (JProperty ee in n["outputs"])
                    {
                        var outputName = ee.Name;
                        var output = ee.Value;
                        foreach (JObject con in output["connections"])
                        {
                            if (!node.outputs.ContainsKey(outputName))
                                node.outputs[outputName] = new List<Connection>();
                            node.outputs[outputName].Add(new Connection()
                            {
                                connector = con["input"].ToObject<string>(),
                                node = (int)con["node"]
                            });
                        }
                    }
                    foreach (JProperty ee in n["inputs"])
                    {
                        var inputName = ee.Name;
                        var input = ee.Value;
                        foreach (JObject con in input["connections"])
                        {
                            if (!node.inputs.ContainsKey(inputName))
                                node.inputs[inputName] = new Connection()
                                {
                                    connector = con["output"].ToObject<string>(),
                                    node = (int)con["node"]
                                };
                        }
                    }
                    node.data = n["data"].ToObject<JObject>();
                    nodes[node.id] = node;
                }

            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        internal bool ContainsTrigger(string triggerObject)
        {
            return nodes.Any(kv => kv.Value.component.name == triggerObject);
        }

        public void trigger(string component, Dictionary<string, object> parameters)
        {
            Node node = nodes.Values.First(n => n.component.name == component);
            node.SetParameters(parameters);
            node.trigger(this);
        }



        public static List<Component> components = new List<Component>();

        public static void init(Service service)
        {
            registerComponent(new TextComponent());
            registerComponent(new TelegramMessageComponent(service));
            registerComponent(new TelegramMessageReceivedComponent());
            registerComponent(new MqttSubscribeComponent());
        }

        static void registerComponent(Component component)
        {
            components.Add(component);
        }

    }
}
