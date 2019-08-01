using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;
using static SensorCloud.services.rulemanager.Service;

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
            var ruleManager = GetService<rulemanager.Service>();
            ruleManager.AddFunction(new Function()
            {
                Module = this.moduleNameFirstCap,
                FunctionName = "SendMessage",
                Parameters = new List<Tuple<string, rules.Socket>>() {
                    new Tuple<string, rules.Socket>("message", new rules.TextSocket()),
                },
                Callback = (async (parameters) => await this.SendMessageAsync((string)parameters["message"]))
            });


            Log("Starting Telegram");
            botClient = new TelegramBotClient(config.bottoken);
            botClient.OnMessage += OnMessage;
            botClient.StartReceiving();
            await SendMessageAsync("Sensorcloud bot started", showNotification: false);
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            GetService<rulemanager.Service>().trigger("Receive Telegram Message", new Dictionary<string, object>()
            {
                {"text" ,e.Message.Text }
            });
            foreach (var item in currentMenu.SubMenus)
            {
                if (e.Message.Text == item.Title)
                {
                    if (item.Callback != null)
                    {
                        var ret = item.Callback();

                        if (ret.returnAfterClick && currentMenu.Parent != null)
                            currentMenu = currentMenu.Parent;
                        else if (item.SubMenus.Count > 0)
                            currentMenu = item;


                        if ((ret.message == null || ret.message == "") && ret.image == null)
                            await SendMessageAsync("Action done");
                        else if(ret.image == null)
                            await SendMessageAsync(ret.message);
                        else
                            await SendPicture(ret.message, ret.image);
                    }
                    else
                    {
                        currentMenu = item;
                        string text = currentMenu.Title + " menu";
                        var afterMenu = currentMenu.AfterMenuText;
                        if (afterMenu != null)
                        {
                            var ret = afterMenu();
                            text += " - " + ret.message;
                            if (ret.image == null)
                                await SendMessageAsync(text);
                            else
                                await SendPicture(text, ret.image);
                        }
                        else
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
            if (botClient == null)
                return;
            List<List<KeyboardButton>> buttons = currentMenu.BuildMenu();


            await botClient.SendTextMessageAsync(
              chatId: config.chatid,
              text: message,
              disableNotification: !showNotification,
              replyMarkup: new ReplyKeyboardMarkup(keyboard: buttons, resizeKeyboard: false, oneTimeKeyboard: false)
            );
        }

        public async Task SendPicture(string caption, Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                List<List<KeyboardButton>> buttons = currentMenu.BuildMenu();
                await botClient.SendPhotoAsync(
                    chatId: config.chatid,
                    caption: caption,
                    photo: stream,
                    replyMarkup: new ReplyKeyboardMarkup(keyboard: buttons, resizeKeyboard: false, oneTimeKeyboard: false)
                    );
            }
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
