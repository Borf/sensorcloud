using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace SensorCloud.modules
{
	/// <summary>
	/// The TelegramModule handles all communication with the Telegram Bot service. It will show a menu to the user
	/// with a keyboard, depending on what menu the user is currently navigating. Can be used by other
	/// modules to send messages to telegram, and other modules can build a menu to show in telegram
	/// </summary>
	class TelegramModule : Module
	{
		private string apiToken;
		private int chatId;
		private TelegramBotClient botClient;

		public Menu rootMenu = new Menu(title: "Root");
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
			SendMessageAsync("Sensorcloud bot started", showNotification: false);
		}

		private void OnMessage(object sender, MessageEventArgs e)
		{
			foreach (var item in currentMenu.SubMenus)
			{
				if (e.Message.Text == item.Title)
				{
					if (item.Callback != null)
					{
						string ret = item.Callback();
						if (ret == "")
							SendMessageAsync("Action done");
						else
							SendMessageAsync(ret);
					}
					else
					{
						currentMenu = item;
						string text = currentMenu.Title + " menu";
						if (currentMenu.AfterMenuText != null)
							text += " - " + currentMenu.AfterMenuText();
						SendMessageAsync(text);
						break;
					}
				}
			}
			if (e.Message.Text == "Back")
			{
				if (currentMenu.Parent != null)
					currentMenu = currentMenu.Parent;
				SendMessageAsync("Went back");
			}


		}

		public void AddRootMenu(Menu menu)
		{
			rootMenu.Add(menu);
		}

		public async void SendMessageAsync(string message, bool showNotification = true)
		{
			List<List<KeyboardButton>> buttons = currentMenu.BuildMenu();

			await botClient.SendTextMessageAsync(
			  chatId: chatId,
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }


	public class Menu
	{
		public string Title { get; private set; }
		public List<Menu> SubMenus { get; private set; } = new List<Menu>();
		public Func<string> Callback { get; private set; }
		public Menu Parent { get; private set; }
		public Func<string> AfterMenuText;

		public Menu(string title, Menu parent = null, Func<string> callback = null, Func<string> afterMenuText = null)
		{
			this.Title = title;
			this.Parent = parent;
			if (parent != null)
				parent.Add(this);
			this.Callback = callback;
			this.AfterMenuText = afterMenuText;

		}
		public Menu(string title, Action callback, Menu parent = null) : this(title, parent, null)
		{
			this.Callback = () => { callback(); return ""; };
		}

		public void Add(Menu menu)
		{
			menu.Parent = this;
			SubMenus.Add(menu);
		}

		internal List<List<KeyboardButton>> BuildMenu()
		{
			List<List<KeyboardButton>> ret = new List<List<KeyboardButton>>();
			List<KeyboardButton> row = new List<KeyboardButton>();
			for (int i = 0; i < SubMenus.Count; i += 2)
			{
				row = new List<KeyboardButton>();
				row.Add(new KeyboardButton(SubMenus[i].Title));
				if (i + 1 < SubMenus.Count)
					row.Add(new KeyboardButton(SubMenus[i + 1].Title));
				ret.Add(row);
			}
			//meh, should be a reference when making the menu object, but don't want to store the telegram module in every menu, and doesn't make a whole lot of sense to pass this as a parameter
			//if (this != ModuleManager.GetModule<TelegramModule>().rootMenu)
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
