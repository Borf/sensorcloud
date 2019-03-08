using Onkyo;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.modules
{
	/// <summary>
	/// The Onkyo module will connect to an onkyo receiver, and publish the relevant information to
	/// the mqtt module. Will also add a telegram menu, if telegram module is loaded
	/// </summary>
	public class OnkyoModule : Module
	{
		private Receiver receiver;
		private MqttModule mqtt;
		private string host;

		public OnkyoModule(string host)
		{
			this.host = host;
		}

		public override async void Start()
		{
			mqtt = GetModule<MqttModule>();
			receiver = new Receiver();
			receiver.Data += onData;
			receiver.VolumeChange += onVolume;
			receiver.PlayStatusChange += onPlayStatus;
			receiver.RepeatStatusChange += onRepeatStatus;
			receiver.ShuffleStatusChange += onShuffleStatus;

			await receiver.Connect(host);

			TelegramModule telegram = GetModule<TelegramModule>();
			if (telegram != null)
				InstallTelegramHandlers(telegram);

			mqtt.On("onkyo/volume/set", (match, payload) => SetVolume(int.Parse(payload)));
			mqtt.On("onkyo/power/set", (match, payload) => SetPower(payload == "on"));
			mqtt.On("onkyo/.*", (m, p) => { });
		}

		private void InstallTelegramHandlers(TelegramModule telegram)
		{
			Menu menu = new Menu(title: "Onkyo");
			Menu power = new Menu(title: "Power", parent: menu);

			power.Add(new Menu(title: "On", callback: () => SetPower(true)));
			power.Add(new Menu(title: "Off", callback: () => SetPower(false)));


			menu.Add(new Menu(title: "Next", callback: () => receiver.Next()));
			menu.Add(new Menu(title: "Previous", callback: () => receiver.Prev()));

			telegram.AddRootMenu(menu);
		}

		private async void onShuffleStatus(object sender, Receiver.ShuffleStatus e)
		{
			switch (e)
			{
				case Receiver.ShuffleStatus.No: await mqtt.Publish("onkyo/status/shuffle", "no", true); break;
				case Receiver.ShuffleStatus.Yes: await mqtt.Publish("onkyo/status/shuffle", "shuffle", true); break;
			}
		}

		private async void onRepeatStatus(object sender, Receiver.RepeatStatus e)
		{
			switch (e)
			{
				case Receiver.RepeatStatus.None: await mqtt.Publish("onkyo/status/repeat", "no", true); break;
				case Receiver.RepeatStatus.One: await mqtt.Publish("onkyo/status/repeat", "single", true); break;
				case Receiver.RepeatStatus.All: await mqtt.Publish("onkyo/status/repeat", "repeat", true); break;
			}
		}

		private async void onPlayStatus(object sender, Receiver.PlayStatus e)
		{
			switch (e)
			{
				case Receiver.PlayStatus.Playing: await mqtt.Publish("onkyo/status", "paused", true); break;
				case Receiver.PlayStatus.Paused: await mqtt.Publish("onkyo/status", "playing", true); break;
				case Receiver.PlayStatus.Stopped: await mqtt.Publish("onkyo/status", "stopped", true); break;
			}
		}




		private async void onVolume(object sender, int newVolume)
		{
			await mqtt.Publish("onkyo/volume", newVolume + "", true);
		}

		private async void onData(object sender, Command cmd)
		{
			//Log($"Got onkyo data: {cmd.command}");
			if (cmd.command == "NTM")
				await mqtt.Publish("onkyo/playtime", cmd.data);
			else
				Log($"Got onkyo data: {cmd.command} -> {cmd.data}");
		}

		public void SetPower(bool status)
		{
			Log("Setting power to " + status);
			receiver.SetPower(status);
		}
		private void SetVolume(int newVolume)
		{
			receiver.SetVolume(newVolume);
		}

	}
}
