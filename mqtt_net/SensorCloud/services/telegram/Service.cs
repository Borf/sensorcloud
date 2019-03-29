using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace SensorCloud.services.telegram
{
    public class Service : SensorCloud.Service
    {
        private TelegramBotClient botClient;
        private Config config;

        public Menu rootMenu = new Menu(title: "Root");
        private Menu currentMenu;


        public Service(IServiceProvider services, Config config) : base(services)
        {
            this.config = config;
            currentMenu = rootMenu;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log("Starting Telegram");
            botClient = new TelegramBotClient(config.bottoken);
            botClient.OnMessage += OnMessage;
            botClient.StartReceiving();
            await SendMessageAsync("Sensorcloud bot started", showNotification: false);
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            foreach (var item in currentMenu.SubMenus)
            {
                if (e.Message.Text == item.Title)
                {
                    if (item.Callback != null)
                    {
                        string ret = item.Callback();
                        if (ret == "")
                            await SendMessageAsync("Action done");
                        else
                            await SendMessageAsync(ret);
                    }
                    else
                    {
                        currentMenu = item;
                        string text = currentMenu.Title + " menu";
                        if (currentMenu.AfterMenuText != null)
                            text += " - " + currentMenu.AfterMenuText();
                        await SendMessageAsync(text);
                        break;
                    }
                }
            }
            if (e.Message.Text == "Back")
            {
                if (currentMenu.Parent != null)
                    currentMenu = currentMenu.Parent;
                await SendMessageAsync("Went back");
            }


        }

        public void AddRootMenu(Menu menu)
        {
            foreach(var m in rootMenu.SubMenus)
            {
                if(m.Title == menu.Title)
                {
                    m.SubMenus.AddRange(menu.SubMenus);
                    if (m.Callback == null && menu.Callback != null)
                        m.Callback = menu.Callback;
                    if (m.AfterMenuText == null && menu.AfterMenuText != null)
                        m.AfterMenuText = menu.Callback;
                    return;
                }
            }
            rootMenu.Add(menu);
        }

        public async Task SendMessageAsync(string message, bool showNotification = true)
        {
            List<List<KeyboardButton>> buttons = currentMenu.BuildMenu();

            await botClient.SendTextMessageAsync(
              chatId: config.chatid,
              text: message,
              disableNotification: !showNotification,
              replyMarkup: new ReplyKeyboardMarkup(keyboard: buttons, resizeKeyboard: false, oneTimeKeyboard: false)
            );
        }

        public bool IsInMenu(Menu menu)
        {
            return currentMenu == menu;
        }

        internal Menu GetRootMenu(string title)
        {
            foreach (Menu m in rootMenu.SubMenus)
                if (m.Title == title)
                    return m;
            return null;
        }
    }
}
