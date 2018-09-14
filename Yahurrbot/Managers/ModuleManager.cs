﻿using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YahurrFramework.Attributes;
using YahurrFramework.Enums;

namespace YahurrFramework.Managers
{
	internal class ModuleManager : BaseManager
	{
		/// <summary>
		/// List of all loaded assmblies
		/// </summary>
		static List<Assembly> LoadedAssemblies;

		public List<YModule> LoadedModules { get; }

		Dictionary<Type, YModule> Modules;

		public ModuleManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			LoadedModules = new List<YModule>();
			LoadedAssemblies = new List<Assembly>();
			Modules = new Dictionary<Type, YModule>();
		}

		/// <summary>
		/// Load all YahurrModules from a folder.
		/// </summary>
		/// <param name="folder">Path to folder containing dll's.</param>
		/// <returns></returns>
		internal async Task LoadModulesAsync(string folder)
		{
			DirectoryInfo directory = Directory.CreateDirectory(folder);
			FileInfo[] files = directory.GetFiles();

			// Load all found modules
			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Loading modules...", "ModuleManager").ConfigureAwait(false);

			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = files[i];

				// New system
				List<Type> Modules = new List<Type>();
				LoadDLL(file.FullName, ref Modules);
				await LoadModules(Modules).ConfigureAwait(false);
			}

			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Loaded {LoadedModules.Count}/{Modules.Count} module{(Modules.Count == 1 ? "" : "s")}...", "ModuleManager").ConfigureAwait(false);
		}

		/// <summary>
		/// Startup all modules.
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

				await module.InitModule().ConfigureAwait(false);
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
				await module.RunMethod("Shutdown");
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
			for (int i = 0; i < LoadedModules.Count; i++)
			{
				YModule module = LoadedModules[i];

				if (!validate(module))
					continue;

				Exception ex = await module.RunMethod(name, parameters).ConfigureAwait(false);

				if (ex != null)
				{
					await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to run method {name}:", "ModuleManager").ConfigureAwait(false);
					await Bot.LoggingManager.LogMessage(ex, "ModuleManager").ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Get any type from this or any loaded assembly.
		/// </summary>
		/// <param name="typeName"></param>
		/// <returns></returns>
		static internal Type GetType(string typeName)
		{
			if (LoadedAssemblies == null)
				return null;

			Type type = Type.GetType(typeName, false, true);
			if (type != null)
				return type;

			for (int i = 0; i < LoadedAssemblies.Count; i++)
			{
				Assembly assembly = LoadedAssemblies[i];
				type = assembly.GetType(typeName, false, true);

				if (type != null)
					return type;
			}

			return null;
		}

		/// <summary>
		/// Load DLL onto memory and extract all YModule classes.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="ModuleTypes"></param>
		void LoadDLL(string path, ref List<Type> ModuleTypes)
		{
			Assembly dll = Assembly.LoadFile(path);
			Type[] types = dll.GetTypes();
			LoadReferences(dll);

			// Add all types that extent yahurrmodule
			for (int i = 0; i < types.Length; i++)
			{
				Type type = types[i];

				// Check if type found is a YahurrModule
				if (typeof(YModule).IsAssignableFrom(type))
				{
					ModuleTypes.Add(type);
				}
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
					await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to load module {moduleType.Name}, type can only have 1 constrcot with 0 arguments.", "ModuleManager").ConfigureAwait(false);
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
				Task.WaitAll(tasks.ConvertAll(a => a.task).ToArray(), 1000, tokenSource.Token);
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

				Modules.Add(type, module);
				AddModule(module);
			});
		}

		/// <summary>
		/// Load all references for a dll
		/// </summary>
		void LoadReferences(Assembly assembly)
		{
			List<Assembly> loadeAssemeblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
			AssemblyName[] names = assembly.GetReferencedAssemblies();

			for (int i = 0; i < names.Length; i++)
			{
				AssemblyName assemblyName = names[i];

				if (!loadeAssemeblies.Exists(a => a.GetName() == assemblyName))
					assembly = AppDomain.CurrentDomain.Load(assemblyName);
			}
		}

		/// <summary>
		/// Bind module to client events.
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
					using (StreamReader reader = new StreamReader(path))
					{
						string json = await reader.ReadToEndAsync().ConfigureAwait(false);
						JToken token = JToken.Parse(json);
						return token.ToObject(configAttribute.Type);
					}
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
	}
}
