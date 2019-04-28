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

namespace SensorCloud.services.mqtt
{
    public class Service : SensorCloud.Service
    {
        private IMqttClient mqttClient;
        private Config config;

        private string lastTopic;
        private string lastValue;


        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();

            mqttClient.Connected += mqttConnect;
            mqttClient.Disconnected += mqttDisconnect;
            mqttClient.ApplicationMessageReceived += mqttMessageReceived;
            Task.Run(async() => await mqttReconnect());
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
            lastTopic = topic;
            lastValue = value;

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
                if(e.ApplicationMessage.Topic != lastTopic || Encoding.UTF8.GetString(e.ApplicationMessage.Payload) != lastValue)
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
