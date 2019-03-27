using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	/// <summary>
	/// Mqtt module connects to an mqtt broker, and allows other modules to subscribe to topics (with regex support)
	/// Will also publish its status on boot/server so that other nodes can see if the server is listening
	/// </summary>
	public class MqttModule : Module
	{
		private IMqttClient mqttClient;
		private string broker;

		public MqttModule(string broker)
		{
			this.broker = broker;
		}


		public override async void Start()
		{
			var factory = new MqttFactory();
			mqttClient = factory.CreateMqttClient();

			mqttClient.Connected += mqttConnect;
			mqttClient.Disconnected += mqttDisconnect;
			mqttClient.ApplicationMessageReceived += mqttMessageReceived;
			await mqttReconnect();
		}

		Dictionary<string, Action<Match, string>> callbacks = new Dictionary<string, Action<Match, string>>();

		internal void On(string topicRegex, Action<Match, string> callback)
		{
			callbacks[topicRegex] = callback;
		}
		internal void Un(string topicRegex)
		{
			callbacks.Remove(topicRegex);
		}

		private async void mqttConnect(object sender, MqttClientConnectedEventArgs e)
		{
			await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());
		}

		public async Task Publish(string topic, string value, bool retain = false)
		{
			while (!mqttClient.IsConnected)
				await Task.Delay(10); //hmm, not so nice
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
				.WithClientId("SensorCloudServer")
				.WithTcpServer(broker)
				.WithWillMessage(new MqttApplicationMessageBuilder()
					.WithTopic("boot/server")
					.WithPayload("dead")
					.WithRetainFlag()
					.Build())
				.Build();

			Log($"Connecting to broker...");
			await mqttClient.ConnectAsync(options);
			Log($"connected");
			await mqttClient.PublishAsync(new MqttApplicationMessageBuilder()
				.WithTopic("boot/server")
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

			if (!handled && !e.ApplicationMessage.Retain)
				Log($"Message on topic {e.ApplicationMessage.Topic} is not handled");
		}

		public void storeLastValue(string topic)
		{
			if (!lastValues.ContainsKey(topic))
				lastValues[topic] = "";
		}

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> lastValues = new Dictionary<string, string>();
	}
}
