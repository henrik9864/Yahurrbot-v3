using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrFramework;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace TestModule
{
	[ServerFilter(FilterType.Whitelist, 288626992373432320), Summary("TestModule")]
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

		[Command("print", "int"), Summary("Prints a int.")]
		public async Task SayInt([Summary("Number to print")]int number)
		{
			await CommandContext.Channel.SendMessageAsync(number.ToString());
		}

		[Command("print", "string"), Summary("Prints a string.")]
		public async Task SayString([Summary("String to print")]string str)
		{
			await CommandContext.Channel.SendMessageAsync(str);
		}

		[Command("add"), Summary("Adds two numbers together.")]
		public async Task AddInt([Summary("Number 1.")]int n1, [Summary("Number 2.")]int n2)
		{
			await CommandContext.Channel.SendMessageAsync((n1 * n2).ToString());
		}

		[Command("param"), Summary("Parameter test")]
		public async Task ParamTest(params string[] strs)
		{
			await CommandContext.Channel.SendMessageAsync("Length: " + strs.Length);

			foreach (var item in strs)
			{
				await CommandContext.Channel.SendMessageAsync("	" + item);
			}

			throw new Exception("Nooo");
		}
	}
}
