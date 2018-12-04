using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using OpenAPI;
using YahurrFramework;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace TestModule
{
	[Config(typeof(ModuleConfig))]
    public class Module1 : YModule
	{
		public new ModuleConfig Config
		{
			get
			{
				return (ModuleConfig)base.Config;
			}
		}

		Spec spec;

		protected override async Task Init()
		{
			spec = Spec.Instance;

			await LogAsync(LogLevel.Message, spec?.basePath ?? "null1");
		}

        protected override async Task Shutdown()
        {
            Console.WriteLine("yay");
            await Task.CompletedTask;
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
				await RespondAsync(Config.PingResponse ?? "Error");
		}

		protected override async Task MessageDeleted(IMessage message, ISocketMessageChannel channel)
		{
			Console.WriteLine("Hello");
			await Task.CompletedTask;
		}

		[Command("print", "int")]
		public async Task SayInt([Summary("Number to print")]int number)
		{
			await RespondAsync(number.ToString()).ConfigureAwait(false);
		}

		[Command("print", "string"), Summary("Prints a string."), Name("Print String")]
		public async Task SayString([Summary("String to print")]string str, int n = 1)
		{
			await RespondAsync($"{n}: {str}").ConfigureAwait(false);
		}

		[Command("add"), Summary("Adds two numbers together.")]
		public async Task AddInt([Summary("Number 1."), Name("Number1")]int n1, [Summary("Number 2.")]int Number2)
		{
			await RespondAsync((n1 * Number2).ToString()).ConfigureAwait(false);
		}

		[Command("param"), Summary("Parameter test")]
		public async Task ParamTest(params string[] strs)
		{
			await RespondAsync("Length: " + strs.Length).ConfigureAwait(false);

			foreach (var item in strs)
			{
				await RespondAsync("	" + item).ConfigureAwait(false);
			}

			throw new Exception("Nooo");
		}

		[Command("param", "noe"), Summary("Parameter test")]
		public async Task ParamTest1(params string[] strs)
		{
			await ParamTest(strs);
		}

		[Command("say", "something")]
		public async void SaySomething()
		{
			await RespondAsync("Hello World!");
			await Task.CompletedTask;
		}

		[Command("say", "something")]
		public async void SaySomething(string text)
		{
			await RespondAsync(text);
			await Task.CompletedTask;
		}
	}
}
