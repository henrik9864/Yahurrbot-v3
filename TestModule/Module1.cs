using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using YahurrBot.Enums;
using YFramework;
using YFramework.Attributes;
using YFramework.Enums;

namespace TestModule
{
	[Config(typeof(ModuleConfig)), RequiredModule(typeof(Module2))]
    public class Module1 : YModule
	{
		public new ModuleConfig Config
		{
			get
			{
				return (ModuleConfig)base.Config;
			}
		}

		protected async override Task MessageReceived(SocketMessage message)
		{
			await SaveAsync("test1", message.Content, true, false).ConfigureAwait(false);
			await SaveAsync("test2", new List<string>() { "s1", "s2" }, true, false).ConfigureAwait(false);
			string msg = await LoadAsync<string>("test1").ConfigureAwait(false);
			var msg1 = await LoadAsync<List<string>>("test2").ConfigureAwait(false);
			bool valid = IsValidAsync<string>("test1");
			bool exists = await ExistsAsync("test1").ConfigureAwait(false);

			if (message.Content == "Ping")
			{
				await message.Channel.SendMessageAsync(Config.PingResponse ?? "Error").ConfigureAwait(false);
			}
		}

		[Command("print", "int")]
		public async Task SayInt([Summary("Number to print")]int number)
		{
			//await Context.Message.Channel.SendMessageAsync(number.ToString());
		}

		[Command("print", "string"), Summary("Prints a string.")]
		public async Task SayString([Summary("String to print")]string str)
		{
			//await Context.Message.Channel.SendMessageAsync(str);
		}

		[ChannelFilter(FilterType.Blacklist, 293381166365540353)]
		[RoleFilter(FilterType.Whitelist, 288627464450736128)]
		[Command("add"), Summary("Adds two numbers together.")]
		public async Task AddInt([Summary("Number 1.")]int n1, [Summary("Number 2.")]int n2)
		{
			//await Context.Message.Channel.SendMessageAsync((n1 * n2).ToString());
		}

		[Command("param"), Summary("Parameter test")]
		public async Task ParamTest(params string[] strs)
		{
			//await Context.Message.Channel.SendMessageAsync("Length: " + strs.Length);

			foreach (var item in strs)
			{
				//await Context.Message.Channel.SendMessageAsync("	" + item);
			}

			throw new Exception("Nooo");
		}
	}
}
