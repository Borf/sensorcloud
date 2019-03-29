using System;
using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;

namespace SensorCloud.services.telegram
{
    public class Menu
    {
        public string Title { get; private set; }
        public List<Menu> SubMenus { get; private set; } = new List<Menu>();
        public Func<string> Callback { get; set; }
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
