﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Onkyo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorCloud.services.onkyo
{
    public class Service : SensorCloud.Service
    {
        private Receiver receiver;
        private mqtt.Service mqtt;
        private string host = "192.168.2.200";


        public Service(IServiceProvider services) : base(services)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            mqtt = GetService<mqtt.Service>();

            receiver = new Receiver();
            receiver.Data += onData;
            receiver.VolumeChange += onVolume;
            receiver.PlayStatusChange += onPlayStatus;
            receiver.RepeatStatusChange += onRepeatStatus;
            receiver.ShuffleStatusChange += onShuffleStatus;
            await mqtt.IsStarted();
            await mqtt.Publish("onkyo/status/title", "", retain: true);
            await mqtt.Publish("onkyo/status/album", "", retain: true);
            await mqtt.Publish("onkyo/status/artist", "", retain: true);


            mqtt.On("onkyo/volume/set", (match, payload) => Volume = int.Parse(payload));
            mqtt.On("onkyo/power/set", (match, payload) => Power = (payload == "on"));
            mqtt.On("onkyo/action", (match, payload) =>
            {
                if (payload == "next")
                {
                    Log("Skipping to next song");
                    receiver.Next();
                }
            });
            mqtt.On("onkyo/.*", (m, p) => { });

            await receiver.Connect(host);

            /* telegram = GetModule<TelegramModule>();
             if (telegram != null)
                 InstallTelegramHandlers(telegram);*/
        }


        public override void HandleCommand(string command)
        {
            string[] cmd = command.Split(" ", 2);
            if (cmd[0] == "cmd")
            {
                receiver.SendCommand(command);
            }
        }





        private async void onShuffleStatus(object sender, ShuffleStatus e)
        {
            switch (e)
            {
                case ShuffleStatus.No: await mqtt.Publish("onkyo/status/shuffle", "no", retain: true); break;
                case ShuffleStatus.Yes: await mqtt.Publish("onkyo/status/shuffle", "shuffle", retain: true); break;
            }
        }

        private async void onRepeatStatus(object sender, RepeatStatus e)
        {
            switch (e)
            {
                case RepeatStatus.None: await mqtt.Publish("onkyo/status/repeat", "no", retain: true); break;
                case RepeatStatus.One: await mqtt.Publish("onkyo/status/repeat", "single", retain: true); break;
                case RepeatStatus.All: await mqtt.Publish("onkyo/status/repeat", "repeat", retain: true); break;
            }
        }

        private async void onPlayStatus(object sender, PlayStatus e)
        {
            switch (e)
            {
                case PlayStatus.Playing: await mqtt.Publish("onkyo/status", "paused", retain: true); break;
                case PlayStatus.Paused: await mqtt.Publish("onkyo/status", "playing", retain: true); break;
                case PlayStatus.Stopped: await mqtt.Publish("onkyo/status", "stopped", retain: true); break;
            }

            if (e == PlayStatus.Playing)
            {
                /*if (telegram?.IsInMenu(onkyoMenu) == true)
                {
                    telegram.SendMessageAsync($"Now playing {receiver.currentSong.index}. {receiver.currentSong.title} ({receiver.currentSong.album} by {receiver.currentSong.artist})", showNotification: false);
                }*/
                await mqtt.Publish("onkyo/status/title", receiver.currentSong.title, retain: true);
                await mqtt.Publish("onkyo/status/album", receiver.currentSong.album, retain: true);
                await mqtt.Publish("onkyo/status/artist", receiver.currentSong.artist, retain: true);
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

        public bool Power {
            get { return receiver.Power; }
            set { receiver.Power = value; }
        }

        public int Volume {
            get { return receiver.Volume; }
            set { receiver.Volume = value; }
        }

        public string Input {
            get { return receiver.Input; }
            set { receiver.Input = value; }
        }

    }
}