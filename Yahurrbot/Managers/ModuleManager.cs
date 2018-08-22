using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YahurrFramework.Attributes;

namespace YahurrFramework.Managers
{
	internal class ModuleManager : BaseManager
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
					await (Task)module.GetType().GetMethod(name).Invoke(module, parameters);
			}
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
			MethodInfo[] methods = module.GetType().GetMethods();
			for (int i = 0; i < methods.Length; i++)
			{
				MethodInfo method = methods[i];

				if (method.GetCustomAttribute<Command>() != null)
					Bot.CommandManager.AddCommand(module, method);
			}

			LoadedModules.Add(module);
		}
	}
}
