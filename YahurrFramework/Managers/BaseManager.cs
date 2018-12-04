using Discord.WebSocket;

namespace YahurrFramework.Managers
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
