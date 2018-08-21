using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework;

namespace TestModule
{
    public class Module : YahurrModule
    {
        public Module(DiscordSocketClient client) : base(client)
		{

		}

		public async override Task MessageReceived(SocketMessage message)
		{
			if (message.Content == "Ping")
			{
				await message.Channel.SendMessageAsync("Pong");
			}
		}
	}
}
