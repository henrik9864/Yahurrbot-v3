using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
		public List<YahurrModule> LoadedModules { get; }

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
			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Loading modules...", "ModuleManager").ConfigureAwait(false);

			for (int i = 0; i < files.Length; i++)
			{
				FileInfo file = files[i];

				// Load and add all modules in dll
				List<YahurrModule> modules = await LoadModule(file.FullName).ConfigureAwait(false);
				AddModules(modules);
			}

			await Bot.LoggingManager.LogMessage(LogLevel.Message, $"Loaded {LoadedModules.Count} module{(LoadedModules.Count == 1 ? "" : "s")}...", "ModuleManager").ConfigureAwait(false);
		}

		/// <summary>
		/// Run a method on all modules
		/// </summary>
		/// <param name="name"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal async Task RunMethod(string name, Func<YahurrModule, bool> validate, params object[] parameters)
		{
			for (int i = 0; i < LoadedModules.Count; i++)
			{
				YahurrModule module = LoadedModules[i];

				if (validate(module))
				{
					Task task = (Task)module.GetType().GetMethod(name).Invoke(module, parameters);

					if (task.Exception != null)
					{
						await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to run method {name}:", "ModuleManager").ConfigureAwait(false);
						await Bot.LoggingManager.LogMessage(task.Exception.InnerException, "ModuleManager").ConfigureAwait(false);
					}
				}
			}
		}

		/// <summary>
		/// Load all YahurrModule classes from a dll file.
		/// </summary>
		/// <param name="path">Full path to dll.</param>
		/// <returns></returns>
		async Task<List<YahurrModule>> LoadModule(string path)
		{
			// List to have all modules in this dll.
			List<YahurrModule> modules = new List<YahurrModule>();
			List<(Task task, string name)> tasks = new List<(Task task, string name)>();

			// Get types from the dll.
			Assembly dll = Assembly.LoadFile(path);
			bool loaded = LoadReferences(dll);
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

					ConstructorInfo[] constructorInfo = type.GetConstructors();
					if (constructorInfo.Length > 1 || constructorInfo[0].GetParameters().Length > 0)
					{
						await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to load module {type.Name}, type can only have 1 constrcot with 0 arguments.", "ModuleManager").ConfigureAwait(false);
						continue;
					}

					// Creat a new task and start running it.
					Task task = new Task(() => {
						YahurrModule module = (YahurrModule)Activator.CreateInstance(type);
						object config = LoadConfig(module).GetAwaiter().GetResult();
						module.InitModule(Client, Bot, config).GetAwaiter().GetResult();
						modules.Add(module);
					});
					task.Start();
					tasks.Add((task, type.Name));
				}
			}

			// Wait for all remaining tasks with 1 second timeout
			CancellationTokenSource tokenSource = new CancellationTokenSource();
			try
			{
				Task.WaitAll(tasks.ConvertAll(a => a.task).ToArray(), 1000, tokenSource.Token);
			}
			catch (Exception ex)
			{
				string taskName = tasks.Find(a => a.task.Exception != null).name;

				Console.WriteLine(ex);

				await Bot.LoggingManager.LogMessage(LogLevel.Error, $"Unable to load module {taskName}:", "ModuleManager").ConfigureAwait(false);
				await Bot.LoggingManager.LogMessage(ex?.InnerException?.InnerException, "ModuleManager").ConfigureAwait(false);
			}

			tokenSource.Cancel();
			return modules;
		}

		/// <summary>
		/// Load all references for a dll
		/// </summary>
		bool LoadReferences(Assembly assembly)
		{
			AssemblyName[] names = assembly.GetReferencedAssemblies();
			for (int i = 0; i < names.Length; i++)
			{
				Assembly.Load(names[i]);
			}

			return true;
		}

		/// <summary>
		/// Load and add all modules to loadedmodules.
		/// </summary>
		/// <param name="modules">Listof modules.</param>
		void AddModules(List<YahurrModule> modules)
		{
			for (int i = 0; i < modules.Count; i++)
			{
				YahurrModule module = modules[i];

				if (!LoadedModules.Exists(a => a.Name == module.Name))
					AddModule(module);
			}
		}

		/// <summary>
		/// Bind module to client events.
		/// </summary>
		/// <param name="module"></param>
		void AddModule(YahurrModule module)
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
		async Task<object> LoadConfig(YahurrModule module)
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
						Console.WriteLine(json);
						return JsonConvert.DeserializeObject(json);
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
