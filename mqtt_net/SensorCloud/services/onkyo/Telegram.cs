using SensorCloud.services.telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.onkyo
{
    public partial class Service
    {
        private Menu onkyoMenu;

        public override void InstallTelegramHandlers(telegram.Service telegram)
        {
            onkyoMenu = new Menu(title: "Onkyo", afterMenuText: () => $"{receiver.currentSong.index}. {receiver.currentSong.title} ({receiver.currentSong.album} by {receiver.currentSong.artist})");

            Menu power = new Menu(title: "Power", parent: onkyoMenu);

            power.Add(new Menu(title: "On", callback: () => Power = true));
            power.Add(new Menu(title: "Off", callback: () => Power = false));


            onkyoMenu.Add(new Menu(title: "Next", callback: () => receiver.Next()));
            onkyoMenu.Add(new Menu(title: "Previous", callback: () => receiver.Prev()));

            Menu inputMenu = new Menu(title: "Input", parent: onkyoMenu, afterMenuText: () => receiver.Input);
            foreach (var i in receiver.getInputs())
            {
                new Menu(title: i, parent: inputMenu, callback: () => { Input = i; });
            }

            Menu volumeMenu = new Menu(title: "Volume", parent: onkyoMenu, afterMenuText: () => "Current volume: " + receiver.Volume);
            new Menu(title: "Down", parent: volumeMenu, callback: () => { return "Current volume: " + --receiver.Volume; });
            new Menu(title: "Up", parent: volumeMenu, callback: () => { return "Current volume: " + ++receiver.Volume; });
            new Menu(title: "Down 5", parent: volumeMenu, callback: () => { receiver.Volume -= 5; return "Current volume: " + (receiver.Volume - 5); });
            new Menu(title: "Up 5", parent: volumeMenu, callback: () => { receiver.Volume += 5; return "Current volume: " + (receiver.Volume + 5); });
            new Menu(title: "Down 10", parent: volumeMenu, callback: () => { receiver.Volume -= 10; return "Current volume: " + (receiver.Volume - 10); });
            new Menu(title: "Up 10", parent: volumeMenu, callback: () => { receiver.Volume += 10; return "Current volume: " + (receiver.Volume + 10); });

            telegram.AddRootMenu(onkyoMenu);
        }
    }
}
