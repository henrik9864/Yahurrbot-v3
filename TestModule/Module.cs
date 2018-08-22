using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace TestModule
{
	[ServerFilter(FilterType.Whitelist, 288626992373432320)]
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

		[Command("print", "int"), Summary("prints a int")]
		public async Task SayInt(int number)
		{
			await CommandContext.Channel.SendMessageAsync(number.ToString());
		}

		[Command("add"), Summary("adds two numbers together")]
		public async Task AddInt(int n1, int n2)
		{
			Console.WriteLine("Running");
			//Console.WriteLine("Context: " + CommandContext);
			await CommandContext.Channel.SendMessageAsync((n1 * n2).ToString());
		}
	}
}
