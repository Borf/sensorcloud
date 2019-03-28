using JvcProjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SensorCloud.services.jvc
{
    public partial class Service
    {
        private telegram.Menu projectorMenu;
        private telegram.Service telegram;

        public override void InstallTelegramHandlers(telegram.Service telegram)
        {
            this.telegram = telegram;

            projectorMenu = new telegram.Menu(title: "Projector", afterMenuText: () => projector.Status.ToString());

            projectorMenu.Add(new telegram.Menu(title: "On", callback: () => projector.Status = PowerStatus.poweron));
            projectorMenu.Add(new telegram.Menu(title: "Off", callback: () => projector.Status = PowerStatus.standby));
            telegram.AddRootMenu(projectorMenu);
        }
    }
}
