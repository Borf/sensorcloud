using api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static SensorCloud.services.rulemanager.Service;

namespace SensorCloud.services.mqtt
{
    public class Service : SensorCloud.Service
    {
        private IMqttClient mqttClient;
        private Config config;

        private List<Tuple<string, string> > lastMessages = new List<Tuple<string, string>>();


        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ruleManager = GetService<rulemanager.Service>();
            ruleManager.AddFunction(new Function()
            {
                Module = this.moduleNameFirstCap,
                FunctionName = "Publish",
                Parameters = new List<Tuple<string, rules.Socket>>() {
                    new Tuple<string, rules.Socket>("topic", new rules.TextSocket()),
                    new Tuple<string, rules.Socket>("payload", new rules.TextSocket())
                },
                Callback = (async (parameters) => await this.Publish((string)parameters["topic"],(string)parameters["payload"]))
            });
            ruleManager.AddTrigger(new Trigger()
            {
                Module = this.moduleNameFirstCap,
                TriggerName = "Subscribe",
                Inputs = new List<Tuple<string, rules.Socket>>()
                {
                    new Tuple<string, rules.Socket>("topic", new rules.TextSocket())
                },
                Outputs = new List<Tuple<string, rules.Socket>>()
                {
                    new Tuple<string, rules.Socket>("payload", new rules.TextSocket())
                },
                Callback = node =>
                {
                    return (string)node.inputValues["topic"] == node.data["in"]["topic"].ToObject<string>();
                }
            });

            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            mqttClient.Connected += mqttConnect;
            mqttClient.Disconnected += mqttDisconnect;
            mqttClient.ApplicationMessageReceived += mqttMessageReceived;
            await Task.Run(async() => await mqttReconnect());
        }


        Dictionary<string, Action<Match, string>> callbacks = new Dictionary<string, Action<Match, string>>();

        public void On(string topicRegex, Action<Match, string> callback)
        {
            callbacks[topicRegex] = callback;
        }
        public void Un(string topicRegex)
        {
            callbacks.Remove(topicRegex);
        }

        private async void mqttConnect(object sender, MqttClientConnectedEventArgs e)
        {
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());
        }

        public async Task Publish(string topic, string value, bool retain = false)
        {
            await IsStarted();
            lastMessages.Add(new Tuple<string, string>(topic, value));
            if (lastMessages.Count > 50)
                lastMessages.RemoveRange(0, lastMessages.Count-50);

            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(value)
                .WithRetainFlag(retain)
                .Build());
        }

        private async void mqttDisconnect(object sender, MqttClientDisconnectedEventArgs e)
        {
            Log("Disconnected from broker");
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    Log($"Reconnecting to broker");
                    await mqttReconnect();
                    Log($"Reconnected to broker");
                    break;
                }
                catch
                {
                    Log($"Reconnecting to MQTT broker failed");
                }
            }
        }

        private async Task mqttReconnect()
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(Startup.IsDevelopment ? "SensorCloudServer_dev" : "SensorCloudServer")
                .WithTcpServer(config.broker)
                .WithWillMessage(new MqttApplicationMessageBuilder()
                    .WithTopic(Startup.IsDevelopment ? "boot/server_dev" : "boot/server")
                    .WithPayload("dead")
                    .WithRetainFlag()
                    .Build())
                .Build();

            Log($"Connecting to broker...");
            await mqttClient.ConnectAsync(options);
            Log($"connected");
            await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
                .WithTopic(Startup.IsDevelopment ? "boot/server_dev" : "boot/server")
                .WithPayload("alive")
                .WithRetainFlag()
                .Build());
        }

        private void mqttMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            if (lastValues.ContainsKey(e.ApplicationMessage.Topic))
                lastValues[e.ApplicationMessage.Topic] = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            GetService<rulemanager.Service>().trigger("Mqtt Subscribe", new Dictionary<string, object>()
            {
                { "topic" , e.ApplicationMessage.Topic },
                { "payload" , Encoding.UTF8.GetString(e.ApplicationMessage.Payload) },

            });

            GetService<rulemanager.Service>().triggerModuleCommand(this.moduleNameFirstCap, "Subscribe", 
                new Dictionary<string, object>()
                {
                    { "topic" , e.ApplicationMessage.Topic },
                    { "payload" , Encoding.UTF8.GetString(e.ApplicationMessage.Payload) },
                });


            bool handled = false;
            foreach (var item in callbacks.ToList())
            {
                Match match = Regex.Match(e.ApplicationMessage.Topic, item.Key);
                if (match.Success)
                {
                    item.Value.Invoke(match, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
                    handled = true;
                }
            }

            if (!handled)
            {
                if(!lastMessages.Contains(new Tuple<string, string>(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload))))
                    Log($"Message on topic {e.ApplicationMessage.Topic} is not handled");
            }
        }

        public void storeLastValue(string topic)
        {
            if (!lastValues.ContainsKey(topic))
                lastValues[topic] = "";
        }

        public async Task IsStarted()
        {
            while (mqttClient == null || mqttClient.IsConnected == false)
                await Task.Delay(10); //hmm, not so nice 
        }


        public Dictionary<string, string> lastValues = new Dictionary<string, string>();
    }
}
