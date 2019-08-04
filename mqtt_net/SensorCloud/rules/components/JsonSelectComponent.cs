using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.rules.components
{
    public class JsonSelectComponent : Component
    {
        public JsonSelectComponent() : base("Json Select")
        {
            addInput("json", new TextSocket());
            addOutput("data", new TextSocket());
            addInput("query", new TextSocket());
        }


        //spotnet.title[0].cookie
        public override void SetOutputs(Node node)
        {
            string query = node.data["query"].ToObject<string>();
            JToken input = JToken.Parse((string)node.getInputValue("json"));
            node.outputValues["data"] = "";
            while (query != "")
            {
                string firstToken = query;
                if (firstToken[0] == '.')
                {
                    query = query.Substring(1);
                    continue;
                }
                if (firstToken[0] == '[')
                {
                    int index;
                    int.TryParse(firstToken.Substring(1), out index);
                    input = input[index];
                    query = query.Substring(query.IndexOf("]") + 1);
                    continue;
                }
                if (firstToken.Contains("."))
                    firstToken = firstToken.Substring(0, firstToken.IndexOf("."));

                input = input[firstToken];
                if (input == null)
                    return;
                query = query.Substring(firstToken.Length);
            }


            node.outputValues["data"] = input.ToString();
        }
    }
}
