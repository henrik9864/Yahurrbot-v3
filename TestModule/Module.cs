using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrBot.Enums;
using YahurrFramework;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace TestModule
{
	[Config(typeof(ModuleConfig))]
    public class Module : YahurrFramework.Module
    {
		public new ModuleConfig Config
		{
			get
			{
				return (ModuleConfig)base.Config;
			}
		}

		public async override Task MessageReceived(SocketMessage message)
		{
			try
			{
				await Save("test1", message.Content, true, false).ConfigureAwait(false);
				string msg = await Load<string>("test1").ConfigureAwait(false);
				bool valid = IsValid<string>("test1");
				bool exists = await Exists("test1").ConfigureAwait(false);

				Console.WriteLine(msg ?? "null");
				Console.WriteLine(exists);
				Console.WriteLine(valid);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

			//Console.WriteLine(msg ?? "null");

			if (message.Content == "Ping")
			{
				await message.Channel.SendMessageAsync(Config?.PingResponse ?? "Error").ConfigureAwait(false);
			}
		}

		[Command("print", "int")]
		public async Task SayInt([Summary("Number to print")]int number)
		{
			await CommandContext.Message.Channel.SendMessageAsync(number.ToString());
		}

		[Command("print", "string"), Summary("Prints a string.")]
		public async Task SayString([Summary("String to print")]string str)
		{
			await CommandContext.Message.Channel.SendMessageAsync(str);
		}

		[ChannelFilter(FilterType.Blacklist, 293381166365540353)]
		[RoleFilter(FilterType.Whitelist, 288627464450736128)]
		[Command("add"), Summary("Adds two numbers together.")]
		public async Task AddInt([Summary("Number 1.")]int n1, [Summary("Number 2.")]int n2)
		{
			await CommandContext.Message.Channel.SendMessageAsync((n1 * n2).ToString());
		}

		[Command("param"), Summary("Parameter test")]
		public async Task ParamTest(params string[] strs)
		{
			await CommandContext.Message.Channel.SendMessageAsync("Length: " + strs.Length);

			foreach (var item in strs)
			{
				await CommandContext.Message.Channel.SendMessageAsync("	" + item);
			}

			throw new Exception("Nooo");
		}
	}
}
