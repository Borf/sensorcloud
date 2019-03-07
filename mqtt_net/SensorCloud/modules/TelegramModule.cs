using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace SensorCloud.modules
{
	class TelegramModule : Module
	{
		private string apiToken;
		private int chatId;
		private TelegramBotClient botClient;

        public Menu rootMenu = new Menu(title : "Root");
        private Menu currentMenu;


		public TelegramModule(string apiToken, int chatId)
		{
			this.apiToken = apiToken;
			this.chatId = chatId;
            currentMenu = rootMenu;
		}

		public override void Start()
		{
			botClient = new TelegramBotClient(apiToken);
			botClient.OnMessage += OnMessage;
			botClient.StartReceiving();
            sendMessage("Sensorcloud bot started");
        }

		private void OnMessage(object sender, MessageEventArgs e)
		{
            foreach(var item in currentMenu.submenus)
            {
                if (e.Message.Text == item.title)
                {
                    if (item.callback != null)
                    {
                        string ret = item.callback();
                        if (ret == "")
                            sendMessage("Action done");
                        else
                            sendMessage(ret);
                    }
                    else
                    {
                        currentMenu = item;
                        sendMessage(currentMenu.title + " menu");
                        break;
                    }
                }
            }
            if (e.Message.Text == "Back")
            {
                if (currentMenu.parent != null)
                    currentMenu = currentMenu.parent;
                sendMessage("Went back");
            }

            /*Log($"Received a text message in chat {e.Message.Chat.Id}.");

			await botClient.SendTextMessageAsync(
			  chatId: e.Message.Chat,
			  text: "You said:\n" + e.Message.Text
			);*/
		}

        public void AddRootMenu(Menu menu)
        {
            rootMenu.Add(menu);
        }

        public async void sendMessage(string message)
		{
            List<List<KeyboardButton>> buttons = currentMenu.BuildMenu();

            await botClient.SendTextMessageAsync(
              chatId: chatId,
              text: message,
              //disableNotification: true,
              replyMarkup: new ReplyKeyboardMarkup(keyboard: buttons, resizeKeyboard: false, oneTimeKeyboard: false)
            );
		}
	}


    public class Menu
    {
        public string title { get; private set; }
        public List<Menu> submenus { get; private set; } = new List<Menu>();
        public Func<string> callback { get; private set; }
        public Menu parent { get; private set; }

        public Menu(string title, Menu parent = null, Func<string> callback = null)
        {
            this.title = title;
            this.parent = parent;
            if (parent != null)
                parent.Add(this);
            this.callback = callback;
        }
        public Menu(string title, Action callback, Menu parent = null) : this(title, parent, null)
        {
            this.callback = () => { callback(); return ""; };
        }

        public void Add(Menu menu)
        {
            menu.parent = this;
            submenus.Add(menu);
        }

        internal List<List<KeyboardButton>> BuildMenu()
        {
            List<List<KeyboardButton>> ret = new List<List<KeyboardButton>>();
            List<KeyboardButton> row = new List<KeyboardButton>();
            for (int i = 0; i < submenus.Count; i+=2)
            {
                row = new List<KeyboardButton>();
                row.Clear();
                row.Add(new KeyboardButton(submenus[i].title));
                if(i+1 < submenus.Count)
                    row.Add(new KeyboardButton(submenus[i+1].title));
                ret.Add(row);
            }
            //meh
            if (this != Module.GetModule<TelegramModule>().rootMenu)
            {
                if (row.Count == 2)
                {
                    row = new List<KeyboardButton>();
                    ret.Add(row);
                }
                row.Add(new KeyboardButton("Back"));
            }


            return ret;
        }
    }


}
