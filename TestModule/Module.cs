﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using YahurrBot.Enums;
using YahurrFramework;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace TestModule
{
	[Config(typeof(ModuleConfig))]
    public class Module : YahurrModule
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
			await Save("test1", new object[] { message.Content, "t1", 2, 's' }, SerializationType.CSV, true).ConfigureAwait(false);
			var msg = await Load<object[]>("test1").ConfigureAwait(false);
			bool valid = await IsValid<object[]>("test1").ConfigureAwait(false);
			bool exists = await Exists("test1").ConfigureAwait(false);

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
