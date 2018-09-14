using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace YFramework.Managers
{
    internal class BaseManager
    {
		protected YahurrBot Bot { get; }

		protected DiscordSocketClient Client { get; }

		public BaseManager(YahurrBot bot, DiscordSocketClient client)
		{
			this.Bot = bot;
			this.Client = client;
		}
    }
}
