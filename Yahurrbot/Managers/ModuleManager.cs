using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YahurrFramework.Managers
{
	class ModuleManager : BaseManager
	{
		public List<YahurrModule> LoadedModules { get; private set; }

		public ModuleManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			LoadedModules = new List<YahurrModule>();
		}

		/// <summary>
		/// Load all YahurrModules from a folder.
		/// </summary>
		/// <param name="folder">Path to folder containing dll's.</param>
		/// <returns></returns>
		internal async Task LoadModules(string folder)
		{
			DirectoryInfo directory = Directory.CreateDirectory(folder);
			FileInfo[] files = directory.GetFiles();

			// Load all found modules
			Console.WriteLine($"Loading modules...");
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = files[i];

				// Load and add all modules in dll
				List<YahurrModule> modules = await Task.Run(() => LoadModule(file.FullName));
				AddModules(modules);
			}

			Console.WriteLine($"Loaded {LoadedModules.Count} module{(LoadedModules.Count == 1 ? "" : "s")}...");
		}

		/// <summary>
		/// Load all YahurrModule classes from a dll file.
		/// </summary>
		/// <param name="path">Full path to dll.</param>
		/// <returns></returns>
		List<YahurrModule> LoadModule(string path)
		{
			// List to have all modules in this dll.
			List<YahurrModule> modules = new List<YahurrModule>();
			List<Task> tasks = new List<Task>();

			// Get types from the dll.
			Assembly dll = Assembly.LoadFile(path);
			LoadReferences(dll);
			Type[] types = dll.GetTypes();

			// Add all types that extent yahurrmodule
			for (int i = 0; i < types.Length; i++)
			{
				Type type = types[i];

				// Check if type found is a YahurrModule
				if (typeof(YahurrModule).IsAssignableFrom(type))
				{
					int index = LoadedModules.FindIndex(a => a.GetType() == type);
					if (index >= 0)
						LoadedModules.RemoveAt(index);

					// Creat a new task and start running it.
					Task task = new Task(() =>
						modules.Add((YahurrModule)Activator.CreateInstance(type, Client)));
					task.Start();
					tasks.Add(task);
				}
			}

			// Wait for all remaining tasks with 1 second timeout
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			Task.WaitAll(tasks.ToArray(), 1000, tokenSource.Token);
			tokenSource.Cancel();

			return modules;
		}

		/// <summary>
		/// Load all references for a dll
		/// </summary>
		void LoadReferences(Assembly assembly)
		{
			AssemblyName[] names = assembly.GetReferencedAssemblies();
			for (int i = 0; i < names.Length; i++)
			{
				Assembly.Load(names[i]);
			}
		}

		/// <summary>
		/// Load and add all modules to loadedmodules.
		/// </summary>
		/// <param name="modules">Listof modules.</param>
		void AddModules(List<YahurrModule> modules)
		{
			for (int i = 0; i < modules.Count; i++)
			{
				AddModule(modules[i]);
			}
		}

		/// <summary>
		/// Bind module to client events.
		/// </summary>
		/// <param name="module"></param>
		void AddModule(YahurrModule module)
		{
			// Bind all methods to events in client.
			#region EventBindings
			Client.GuildUpdated += (a, b) => Task.Run(() => module.GuildUpdated(a, b));
			Client.ReactionRemoved += (a, b, c) => Task.Run(() => module.ReactionRemoved(a.Value, b, c));
			Client.ReactionsCleared += (a, b) => Task.Run(() => module.ReactionsCleared(a.Value, b));
			Client.RoleCreated += (a) => Task.Run(() => module.RoleCreated(a));
			Client.RoleDeleted += (a) => Task.Run(() => module.RoleDeleted(a));
			Client.RoleUpdated += (a, b) => Task.Run(() => module.RoleUpdated(a, b));
			Client.JoinedGuild += (a) => Task.Run(() => module.JoinedGuild(a));
			Client.UserIsTyping += (a, b) => Task.Run(() => module.UserIsTyping(a, b));
			Client.CurrentUserUpdated += (a, b) => Task.Run(() => module.CurrentUserUpdated(a, b));
			Client.UserVoiceStateUpdated += (a, b, c) => Task.Run(() => module.UserVoiceStateUpdated(a, b, c));
			Client.GuildMemberUpdated += (a, b) => Task.Run(() => module.GuildMemberUpdated(a, b));
			Client.UserUpdated += (a, b) => Task.Run(() => module.UserUpdated(a, b));
			Client.UserUnbanned += (a, b) => Task.Run(() => module.UserUnbanned(a, b));
			Client.UserBanned += (a, b) => Task.Run(() => module.UserBanned(a, b));
			Client.ReactionAdded += (a, b, c) => Task.Run(() => module.ReactionAdded(a.Value, b, c));
			Client.LeftGuild += (a) => Task.Run(() => module.LeftGuild(a));
			Client.GuildAvailable += (a) => Task.Run(() => module.GuildAvailable(a));
			Client.GuildUnavailable += (a) => Task.Run(() => module.GuildUnavailable(a));
			Client.GuildMembersDownloaded += (a) => Task.Run(() => module.GuildMembersDownloaded(a));
			Client.UserJoined += (a) => Task.Run(() => module.UserJoined(a));
			Client.MessageUpdated += (a, b, c) => Task.Run(() => module.MessageUpdated(a.Value, b, c));
			Client.LatencyUpdated += (a, b) => Task.Run(() => module.LatencyUpdated(a, b));
			Client.MessageReceived += (a) => Task.Run(() => module.MessageReceived(a));
			Client.MessageDeleted += (a, b) => Task.Run(() => module.MessageDeleted(a.Value, b));
			Client.Connected += () => Task.Run(() => module.Connected());
			Client.Disconnected += (a) => Task.Run(() => module.Disconnected(a));
			Client.Ready += () => Task.Run(() => module.Ready());
			Client.RecipientRemoved += (a) => Task.Run(() => module.RecipientRemoved(a));
			Client.ChannelCreated += (a) => Task.Run(() => module.ChannelCreated(a));
			Client.ChannelDestroyed += (a) => Task.Run(() => module.ChannelDestroyed(a));
			Client.ChannelUpdated += (a, b) => Task.Run(() => module.ChannelUpdated(a, b));
			Client.RecipientAdded += (a) => Task.Run(() => module.RecipientAdded(a));
			#endregion

			LoadedModules.Add(module);
		}
	}
}
