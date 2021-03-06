﻿using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace YahurrFramework.Managers
{
	internal class ModuleManager : BaseManager
	{
		public List<YModule> LoadedModules { get; }

		Dictionary<Type, YModule> Modules;

		public ModuleManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			LoadedModules = new List<YModule>();
			Modules = new Dictionary<Type, YModule>();
		}

		/// <summary>
		/// Load all DLL's into memor and instansiate all modules
		/// </summary>
		/// <param name="folder">Path to folder containing dll's.</param>
		/// <returns></returns>
		internal async Task LoadModulesAsync(string folder)
		{
			DirectoryInfo modulesDirectory = Directory.CreateDirectory(folder);
			DirectoryInfo[] directories = modulesDirectory.GetDirectories();

			// Load all found modules
			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Loading modules...", "ModuleManager").ConfigureAwait(false);

			List<Assembly> ModuleAssemblies = new List<Assembly>();
			for (int i = 0; i < directories.Length; i++)
			{
				DirectoryInfo dir = directories[i];

				string modulePath = $"{dir.FullName}/{dir.Name}.dll";
				if (!File.Exists(modulePath))
				{
					await Bot.LoggingManager.LogMessage(LogLevel.Warning, $"Unable to load module {dir.Name} DLL not found.", "Module Manager");
					continue;
				}

				ModuleAssemblies.Add(await Bot.AssemblyManager.LoadDLL($"{dir.FullName}/{dir.Name}.dll"));
			}

			List<Type> Modules = new List<Type>();
			for (int i = 0; i < ModuleAssemblies.Count; i++)
			{
				Assembly moduleAssembly = ModuleAssemblies[i];
				Type[] types = moduleAssembly.GetTypes();

				for (int a = 0; a < types.Length; a++)
				{
					Type type = types[a];

					// Check if type found is a YahurrModule
					if (typeof(YModule).IsAssignableFrom(type))
						Modules.Add(type);
				}
			}

			await LoadModules(Modules).ConfigureAwait(false);
			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Loaded {LoadedModules.Count}/{Modules.Count} module{(Modules.Count == 1 ? "" : "s")}", "ModuleManager").ConfigureAwait(false);
		}

		/// <summary>
		/// Run init on all modules and create ConfigFiles.
		/// </summary>
		/// <returns></returns>
		internal async Task InitializeModules()
		{
			for (int i = 0; i < LoadedModules.Count; i++)
			{
				YModule module = LoadedModules[i];
				RequiredModule requiredModule = module.GetType().GetCustomAttribute<RequiredModule>();

				if (requiredModule != null)
				{
					for (int a = 0; a < requiredModule.Types.Count; a++)
					{
						Type type = requiredModule.Types[a];

						if (!LoadedModules.Exists(m => m.GetType() == type))
						{
							await Bot.LoggingManager.LogMessage(LogLevel.Warning, $"Unable to initalize module {module.Name} it requires {type.Name} wich is not loaded.", "ModuleManager").ConfigureAwait(false);
							return;
						}
					}
				}

				module.SetContext(new MethodContext(Client.Guilds.FirstOrDefault(), null, null));
				await module.InitModule().ConfigureAwait(false);
			}

			for (int i = 0; i < LoadedModules.Count; i++)
			{
				YModule module = LoadedModules[i];
				await module.ModuleDone();
			}
		}

		/// <summary>
		/// Shut down all modules.
		/// </summary>
		/// <returns></returns>
		internal async Task ShutdownModules()
		{
			for (int i = 0; i < LoadedModules.Count; i++)
			{
				YModule module = LoadedModules[i];
				await Task.Run(() => module.RunCommand("Shutdown", 0)).ConfigureAwait(false); // Make sure one slow shutdown stops every other module from getting the call
			}
		}

		/// <summary>
		/// Run a method on all modules
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal async Task RunMethod(string name, Func<YModule, bool> validate, params object[] parameters)
		{
			IGuild guild = null;
			ISocketMessageChannel channel = null;
			IMessage message = null;

			foreach (object item in parameters)
			{
				if (item is IGuild)
					guild = item as IGuild;

				if (item is ISocketMessageChannel)
					channel = item as ISocketMessageChannel;

				if (item is IMessage)
				{
					message = item as IMessage;
					channel = message?.Channel as ISocketMessageChannel;
				}
			}

			MethodContext context = new MethodContext(guild, channel, message);

			for (int i = 0; i < LoadedModules.Count; i++)
			{
				YModule module = LoadedModules[i];

				if (!validate(module))
					continue;

				try
				{
					module.SetContext(context);
					await module.RunCommand(name, parameters.Length, parameters).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to run method {name}:", "ModuleManager").ConfigureAwait(false);
					await Bot.LoggingManager.LogMessage(ex, "ModuleManager").ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Reload config from file for all loaded modules.
		/// </summary>
		/// <returns></returns>
		internal async Task ReloadConfig()
		{
			foreach (YModule module in LoadedModules)
			{
				object config = await LoadConfig(module);
				module.ChangeConfig(config);
			}
		}

		/// <summary>
		/// Save all configs to their respective files.
		/// </summary>
		/// <returns></returns>
		internal async Task SaveConfig()
		{
			foreach (YModule module in LoadedModules)
			{
				await SaveConfig(module);
			}
		}

		/// <summary>
		/// Create an instance of all modues in list.
		/// </summary>
		/// <param name="Modules">Modules to create an instance of.</param>
		/// <returns></returns>
		async Task LoadModules(List<Type> Modules)
		{
			List<YModule> YModules = new List<YModule>();
			List<(Task task, string name)> tasks = new List<(Task task, string name)>();

			for (int i = 0; i < Modules.Count; i++)
			{
				Type moduleType = Modules[i];

				ConstructorInfo[] constructorInfo = moduleType.GetConstructors();
				if (constructorInfo.Length > 1 || constructorInfo[0].GetParameters().Length > 0)
				{
					await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to load module {moduleType.Name}, type can only have 1 constrctor with 0 arguments.", "ModuleManager").ConfigureAwait(false);
					continue;
				}

				// Creat a new task and start running it.
				Task task = LoadModule(moduleType);
				//task.Start();
				tasks.Add((task, moduleType.Name));
			}

			// Wait for all remaining tasks with 1 second timeout
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			try
			{
				Task.WaitAll(tasks.ConvertAll(a => a.task).ToArray(), 10000, tokenSource.Token);
			}
			catch (Exception ex)
			{
				for (int i = 0; i < tasks.Count; i++)
				{
					(Task task, string name) task = tasks[i];

					if (task.task.Exception == null)
						continue;

					await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to load module {task.name}:", "ModuleManager").ConfigureAwait(false);
					await Bot.LoggingManager.LogMessage(ex?.InnerException, "ModuleManager").ConfigureAwait(false);
				}
			}

			tokenSource.Cancel();
		}

		/// <summary>
		/// Create instance of module.
		/// </summary>
		/// <param name="type">Module type.</param>
		/// <returns></returns>
		Task LoadModule(Type type)
		{
			return Task.Run(() =>
			{
				YModule module = (YModule)Activator.CreateInstance(type);
				object config = LoadConfig(module).GetAwaiter().GetResult();
				module.LoadModule(Client, Bot, config);

				lock (Modules)
					Modules.Add(type, module);

				lock (LoadedModules)
					AddModule(module);
			});
		}

		/// <summary>
		/// Load all commands from module.
		/// </summary>
		/// <param name="module"></param>
		void AddModule(YModule module)
		{
			MethodInfo[] methods = module.GetType().GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo method = methods[i];

				if (method.GetCustomAttribute<Command>() != null)
					Bot.CommandManager.AddCommand(module, method);
			}

			LoadedModules.Add(module);
		}

		/// <summary>
		/// Load or create config for module.
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		async Task<object> LoadConfig(YModule module)
		{
			Config configAttribute = module.GetType().GetCustomAttribute<Config>();

			if (configAttribute != null)
			{
				Directory.CreateDirectory("Config/Modules");

				string path = $"Config/Modules/{module.Name}.json";
				if (File.Exists(path))
				{
                    object config;
                    string json;
					using (StreamReader reader = new StreamReader(path))
					{
						json = await reader.ReadToEndAsync().ConfigureAwait(false);
                        config = JToken.Parse(json).ToObject(configAttribute.Type);
					}

                    // Update config file JSON in case there is a new parameter
                    json = JsonConvert.SerializeObject(config, Formatting.Indented);
                    await File.WriteAllTextAsync(path, json);
                    return config;
                }
				else
				{
					object instance = Activator.CreateInstance(configAttribute.Type);
					string json = JsonConvert.SerializeObject(instance, Formatting.Indented);
					using (StreamWriter writer = File.CreateText(path))
					{
						await writer.WriteLineAsync(json).ConfigureAwait(false);
					}

					return instance;
				}
			}

			return null;
		}

		async Task SaveConfig(YModule module)
		{
			string path = $"Config/Modules/{module.Name}.json";
			string json = JsonConvert.SerializeObject(module.GetConfig(), Formatting.Indented);

			await File.WriteAllTextAsync(path, json);
		}
	}
}
