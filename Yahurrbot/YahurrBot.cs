using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using YahurrFramework.Managers;
using YahurrFramework.Structs;

namespace YahurrFramework
{
    public class YahurrBot
    {
		/// <summary>
		/// What type of bot this is.
		/// </summary>
		public TokenType Type { get; } = TokenType.Bot;

		DiscordSocketClient client;
		List<YahurrModule> loadedModules;

		ModuleManager moduleManager;

		public YahurrBot()
		{
			client = new DiscordSocketClient();
			loadedModules = new List<YahurrModule>();
			moduleManager = new ModuleManager(this, client);
		}

		/// <summary>
		/// Start Yahurrbot
		/// </summary>
		/// <returns></returns>
		public async Task StartAsync()
		{
			// Run Yahurrbot startup
			Console.WriteLine("Starting Yahurrbot v0.0.1");
			await StartupAsync();

			// Load all modules onto memory
			await moduleManager.LoadModules("Modules");

			// Continue this as main loop.
			Console.WriteLine("Done.");
			await Task.Delay(-1);
		}

		/// <summary>
		/// Stop Yahurrbot
		/// </summary>
		/// <returns></returns>
		public async Task StopAsync()
		{
			await client.StopAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Run startup procedure for Yahurrbot.
		/// </summary>
		/// <returns></returns>
		async Task StartupAsync()
		{
			Console.WriteLine("Loading tokens...");
			ClientInfo clientInfo = await GetInfoAsync("Tokens/YahurrToken.json").ConfigureAwait(false);

			Console.WriteLine("Starting bot...");
			await client.LoginAsync(Type, clientInfo.Token).ConfigureAwait(false);
			await client.StartAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Retrive info to start discord bot from file.
		/// </summary>
		/// <param name="filePath">Path to file containing the tokens.</param>
		/// <returns></returns>
		async Task<ClientInfo> GetInfoAsync(string filePath)
		{
			using (StreamReader reader = new StreamReader(filePath))
			{
				string json = await reader.ReadToEndAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<ClientInfo>(json);
			}
		}
	}
}
