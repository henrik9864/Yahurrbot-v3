using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using YahurrFramework.Enums;
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

		public string Version { get; } = "0.0.1";

		internal ModuleManager ModuleManager { get; }

		internal EventManager EventManager { get; }

		internal CommandManager CommandManager { get; }

		internal LoggingManager LoggingManager { get; }

		DiscordSocketClient client;

		public YahurrBot()
		{
			client = new DiscordSocketClient();

			ModuleManager = new ModuleManager(this, client);
			EventManager = new EventManager(this, client);
			CommandManager = new CommandManager(this, client);
			LoggingManager = new LoggingManager(this, client);

			LoggingManager.Log += Log;
		}

		/// <summary>
		/// Start Yahurrbot
		/// </summary>
		/// <returns></returns>
		public async Task StartAsync()
		{
			// Run Yahurrbot startup
			await LoggingManager.LogMessage(LogLevel.Message, $"Starting Yahurrbot v{Version}", "Startup").ConfigureAwait(false);
			await StartupAsync();

			// Load all modules onto memory
			await ModuleManager.LoadModules("Modules");

			// Continue this as main loop.
			await LoggingManager.LogMessage(LogLevel.Message, $"Done", "Startup").ConfigureAwait(false);

			// Run command and main loop
			Task.Run(CommandLoop);
			await MainLoop();
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
		/// Main yahurrbot loop
		/// </summary>
		/// <returns></returns>
		async Task MainLoop()
		{
			await Task.Delay(-1);
		}

		/// <summary>
		/// Bot command loop
		/// </summary>
		/// <returns></returns>
		async Task CommandLoop()
		{
			while (true)
			{
				string input = Console.ReadLine().ToLower();
				string[] commands = input.Split(' ');

				if (commands.Length == 0)
					continue;

				switch (commands[0])
				{
					case "reload":
						string folder = "Modules";
						if (commands.Length > 1)
							folder = commands[1];

						await ModuleManager.LoadModules(folder);
						break;
					case "list":
						Console.WriteLine("Loaded modules:");
						for (int i = 0; i < ModuleManager.LoadedModules.Count; i++)
						{
							Console.WriteLine("	" + ModuleManager.LoadedModules[i].GetType().Name);
						}
						break;
					case "exit":
						await StopAsync();
						Environment.Exit(1);
						return;
					default:
						Console.WriteLine($"Unknown command: {commands[0]}");
						break;
				}
			}
		}

		/// <summary>
		/// Run startup procedure for Yahurrbot.
		/// </summary>
		/// <returns></returns>
		async Task StartupAsync()
		{
			await LoggingManager.LogMessage(LogLevel.Message, $"Loading tokens...", "Startup").ConfigureAwait(false);
			ClientInfo clientInfo = await GetInfoAsync("Tokens/YahurrToken.json").ConfigureAwait(false);

			await LoggingManager.LogMessage(LogLevel.Message, $"Starting bot...", "Startup").ConfigureAwait(false);
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

		async Task Log(LogMessage message)
		{
			Console.WriteLine($"{message.LogLevel}: {message.Message}");
		}
	}
}
