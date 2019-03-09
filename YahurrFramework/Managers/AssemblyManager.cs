using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace YahurrFramework.Managers
{
	internal class AssemblyManager : BaseManager
	{
		Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

		public AssemblyManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoad;
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
			AppDomain.CurrentDomain.TypeResolve += TypeResolve;
		}

		/// <summary>
		/// Load DLL and its dependencies
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<Assembly> LoadDLL(string path)
		{
			string assemblyName = Path.GetFileNameWithoutExtension(path);
			string assemblyPath = Path.GetDirectoryName(path);

			// Load symbos for assembly if it exists.
			byte[] rawSymbolStore = null;
			string symbolStorePath = $"{assemblyPath}/{assemblyName}.pdb";
			if (File.Exists(symbolStorePath))
				rawSymbolStore = await LoadFileAsync(symbolStorePath);

			byte[] rawAssembly = await LoadFileAsync(path);

			Assembly assembly;
			if (rawSymbolStore != null)
				assembly = AppDomain.CurrentDomain.Load(rawAssembly, rawSymbolStore);
			else
				assembly = AppDomain.CurrentDomain.Load(rawAssembly);

			// Load all DLL refernces.
			AssemblyName[] refernces = assembly.GetReferencedAssemblies();
			foreach (AssemblyName reference in refernces)
			{
				string referncePath = $"{assemblyPath}/{reference.Name}.dll";

				if (File.Exists(referncePath))
					await LoadDLL(referncePath);
			}

			return assembly;
		}

		Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assemblies.TryGetValue(args.Name, out Assembly assembly);
			return assembly;
		}

		Assembly TypeResolve(object sender, ResolveEventArgs args)
		{
			foreach (KeyValuePair<string, Assembly> item in Assemblies)
			{
				Type type = item.Value.GetType(args.Name);

				return item.Value;
			}

			return null;
		}

		void AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			Assemblies.Add(args.LoadedAssembly.FullName, args.LoadedAssembly);
		}

		async Task<byte[]> LoadFileAsync(string path)
		{
			byte[] buffer;
			using (FileStream fileStream = new FileStream(path, FileMode.Open))
			{
				buffer = new byte[(int)fileStream.Length];
				await fileStream.ReadAsync(buffer, 0, buffer.Length);
			}

			return buffer;
		}
	}
}
