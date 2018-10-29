using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using YahurrBot.Enums;
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

		public string Version { get; } = "1.1.0";

		internal ClientConfig Config { get; private set; } = new ClientConfig();

		internal ModuleManager ModuleManager { get; }

		internal EventManager EventManager { get; }

		internal CommandManager CommandManager { get; }

		internal LoggingManager LoggingManager { get; }

		internal FileManager FileManager { get; }

		DiscordSocketClient client;

		public YahurrBot()
		{
			client = new DiscordSocketClient();

			LoggingManager = new LoggingManager(this, client);
			CommandManager = new CommandManager(this, client);
			ModuleManager = new ModuleManager(this, client);
			EventManager = new EventManager(this, client);
			FileManager = new FileManager(this, client);

			LoggingManager.Log += Log;
			LoggingManager.Read += GetInput;
			AppDomain.CurrentDomain.ProcessExit += async (a, b) => await StopAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Start Yahurrbot
		/// </summary>
		/// <returns></returns>
		public async Task<ReturnCode> StartAsync()
		{
			// Run Yahurrbot startup
			await LoggingManager.LogMessage(LogLevel.Message, $"Starting Yahurrbot v{Version}", "Startup").ConfigureAwait(false);
			bool succsess = await StartupAsync().ConfigureAwait(false);

			if (!succsess)
				return ReturnCode.Error;

			// Load all modules onto memory
			await ModuleManager.LoadModulesAsync("Modules").ConfigureAwait(false);

			// Load all savefiles
			await LoggingManager.LogMessage(LogLevel.Message, $"Loading save files...", "Startup").ConfigureAwait(false);
			await FileManager.LoadObjectList().ConfigureAwait(false);

			// Start all loaded modules
			await LoggingManager.LogMessage(LogLevel.Message, $"Initializing modules...", "Startup").ConfigureAwait(false);
			await ModuleManager.InitializeModules().ConfigureAwait(false);

			// Continue this as main loop.
			await LoggingManager.LogMessage(LogLevel.Message, $"Done", "Startup").ConfigureAwait(false);

			// Run command and main loop
			return await CommandLoop().ConfigureAwait(false);
		}

		/// <summary>
		/// Stop Yahurrbot
		/// </summary>
		/// <returns></returns>
		public async Task StopAsync()
		{
			await LoggingManager.LogMessage(LogLevel.Message, "Shutting down Yahurrbot...", "Shutdown").ConfigureAwait(false);

            await ModuleManager.ShutdownModules().ConfigureAwait(false);
            await client.StopAsync().ConfigureAwait(false);

			await LoggingManager.LogMessage(LogLevel.Message, "Goodbye.", "Shutdown").ConfigureAwait(false);
			await Task.Delay(1000);
		}

		/// <summary>
		/// Bot command loop
		/// </summary>
		/// <returns></returns>
		async Task<ReturnCode> CommandLoop()
		{
			while (true)
			{
				string input = Console.ReadLine().ToLower();
				string[] commands = input.Split(' ');

				if (commands.Length == 0)
					continue;

				switch (input)
				{
					case "reload config":
						string folder = "Config";
						if (commands.Length > 1)
							folder = commands[1];

						Config = await GetConfig(folder).ConfigureAwait(false);
						break;
					case "list":
						Console.WriteLine("Loaded modules:");
						for (int i = 0; i < ModuleManager.LoadedModules.Count; i++)
						{
							Console.WriteLine("	" + ModuleManager.LoadedModules[i].GetType().Name);
						}
						break;
					case "exit":
                        Environment.Exit(1);
                        return ReturnCode.OK;
					case "reboot":
						await StopAsync().ConfigureAwait(false);
						return ReturnCode.Reboot;
					default:
						Console.WriteLine($"Unknown command: {commands[0]}");
						break;
				}
			}
		}

		/// <summary>
		/// Run startup procedure for Yahurrbot.
		/// </summary>
		/// <returns>If startup was sucsessfull</returns>
		async Task<bool> StartupAsync()
		{
			await LoggingManager.LogMessage(LogLevel.Message, $"Loading config...", "Startup").ConfigureAwait(false);
			Config = await GetConfig("Config").ConfigureAwait(false);

			if (Config == null)
			{
				await LoggingManager.LogMessage(LogLevel.Critical, $"Unable to load a config from folder 'Config'", "Startup").ConfigureAwait(false);
				return false;
			}

			await LoggingManager.LogMessage(LogLevel.Message, $"Loading tokens...", "Startup").ConfigureAwait(false);
			ClientToken clientInfo = await GetToken(Config.TokenDirectory ?? "").ConfigureAwait(false);

			if (clientInfo == null)
			{
				await LoggingManager.LogMessage(LogLevel.Critical, $"Unable to load token from folder '{Config.TokenDirectory}'", "Startup").ConfigureAwait(false);
				return false;
			}

			await LoggingManager.LogMessage(LogLevel.Message, $"Starting bot...", "Startup").ConfigureAwait(false);
			await client.LoginAsync(Type, clientInfo.Token).ConfigureAwait(false);
			await client.StartAsync().ConfigureAwait(false);

			return true;
		}

		/// <summary>
		/// Retrive info to start discord bot from file.
		/// </summary>
		/// <param name="directory">Path to folder containing the token files.</param>
		/// <returns></returns>
		async Task<ClientToken> GetToken(string directory)
		{
			Directory.CreateDirectory(directory);
			JSchemaGenerator generator = new JSchemaGenerator();
			JSchema schema = generator.Generate(typeof(ClientToken));

			string file = await GetFileOfType(directory, "*.json", Config.DefaultTokenIndex).ConfigureAwait(false);

			if (string.IsNullOrEmpty(file))
				return null;

			using (StreamReader reader = new StreamReader(file))
			{
				string json = await reader.ReadToEndAsync().ConfigureAwait(false);
				JObject token = JObject.Parse(json);

				if (token.IsValid(schema))
					return token.ToObject<ClientToken>();

				await LoggingManager.LogMessage(LogLevel.Message, "Invalid token file.", "Startup").ConfigureAwait(false);
				return null;
			}
		}

		/// <summary>
		/// Retrives config from file.
		/// </summary>
		/// <param name="directory">Directory containing all config files.</param>
		/// <returns></returns>
		async Task<ClientConfig> GetConfig(string directory)
		{
			string file = await GetFileOfType(directory, "*.json").ConfigureAwait(false);

			if (string.IsNullOrEmpty(file))
				return null;

			using (StreamReader reader = new StreamReader(file))
			{
				string json = await reader.ReadToEndAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<ClientConfig>(json);
			}
		}

		/// <summary>
		/// Get file of type with inbuild error messages.
		/// </summary>
		/// <param name="directory">Directory to pick from.</param>
		/// <param name="format">What files to include.</param>
		/// <param name="searchOption">Where to search for files.</param>
		/// <returns>File</returns>
		async Task<string> GetFileOfType(string directory, string format, int deafultIndex = -1, SearchOption searchOption = SearchOption.TopDirectoryOnly)
		{
			int index = deafultIndex;
			string[] files = Directory.GetFiles(directory, format, searchOption);

			if (files.Length > 1 && index == -1)
			{
				await LoggingManager.LogMessage(LogLevel.Message, "Please type file name or index.", "Startup").ConfigureAwait(false);
				string nameOrIndex = await LoggingManager.GetInput().ConfigureAwait(false);

				if (string.IsNullOrEmpty(nameOrIndex))
				{
					await LoggingManager.LogMessage(LogLevel.Message, "Invalid input.", "Startup").ConfigureAwait(false);
					return null;
				}

				if (!int.TryParse(nameOrIndex, out index))
				{
					index = Array.FindIndex(files, a =>
						a.IndexOf(nameOrIndex, StringComparison.OrdinalIgnoreCase) >= 0
					);
				}

				if (index == -1 || index >= files.Length)
				{
					await LoggingManager.LogMessage(LogLevel.Message, "Name or index not found.", "Startup").ConfigureAwait(false);
					return null;
				}
			}
			else if (files.Length == 1)
				return files[0];
			else if (files.Length == 0)
				return null;
			else
				await LoggingManager.LogMessage(LogLevel.Message, $"Using default index of {Config.DefaultTokenIndex}", "Startup").ConfigureAwait(false);

			return files[index];
		}

		Task Log(LogMessage message, ClientConfig config)
		{
			if (config == null)
			{
				Console.WriteLine($"{message.Timestamp.ToString("hh:mm:ss")} {message.Source}: {message.Message}");
				return Task.CompletedTask;
			}

			if (message.LogLevel < config.MinLogLevel)
				return Task.CompletedTask;

			if (message.Exception != null && config.ThrowExceptions)
				Console.WriteLine(message.Exception);
			else
				Console.WriteLine($"{message.Timestamp.ToString("HH:mm:ss")} {message.Source}: {message.Message}");

			return Task.CompletedTask;
		}

		Task<string> GetInput(ClientConfig config)
		{
			return Task.Run(() => Console.ReadLine());
		}
	}
}
