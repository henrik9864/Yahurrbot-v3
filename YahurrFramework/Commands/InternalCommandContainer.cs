using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Commands
{
	internal class InternalCommandContainer : CommandContainer
	{
		protected DiscordSocketClient Client { get; }

		protected YahurrBot Bot { get; }

		public InternalCommandContainer(DiscordSocketClient client, YahurrBot bot)
		{
			Client = client;
			Bot = bot;
		}
	}
}
