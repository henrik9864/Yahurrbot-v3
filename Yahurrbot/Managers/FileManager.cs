using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using YahurrBot.Structs;

namespace YahurrFramework.Managers
{
	internal class FileManager : BaseManager
	{
		Dictionary<(string name, YahurrModule module), SavedObject> cache;

		public FileManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			cache = new Dictionary<(string name, YahurrModule module), SavedObject>();
			Directory.CreateDirectory("Saves");
		}

		/// <summary>
		/// Sav object to file.
		/// </summary>
		/// <param name="obj">Object to save</param>
		/// <param name="name">Reference for loading this object.</param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <returns></returns>
		public async Task Save(object obj, string name, YahurrModule module, bool @override = true)
		{
			SavedObject savedObject = new SavedObject(name, obj);
			string json = JsonConvert.SerializeObject(savedObject);

			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");
			string path = $"Saves/{SanetizeName(module.Name)}/{name}.json";
			if (!File.Exists(path))
				using (StreamWriter writer = File.CreateText(path))
					await writer.WriteAsync(json).ConfigureAwait(false);
			else if (@override)
				using (StreamWriter writer = new StreamWriter(path))
					await writer.WriteAsync(json).ConfigureAwait(false);

			if (cache.TryGetValue((name, module), out SavedObject so))
				if (@override)
					cache[(name, module)] = savedObject;
			else
				cache.Add((name, module), savedObject);
		}

		/// <summary>
		/// Load object from cache or file.
		/// </summary>
		/// <typeparam name="T">Class you have saved</typeparam>
		/// <param name="name">Identefier</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public async Task<T> Load<T>(string name, YahurrModule module)
		{
			if (!cache.TryGetValue((name, module), out SavedObject savedObject))
			{
				string path = $"Saves/{SanetizeName(module.Name)}/{name}.json";
				using (StreamReader reader = new StreamReader(path))
				{
					string json = await reader.ReadToEndAsync().ConfigureAwait(false);
					savedObject = JsonConvert.DeserializeObject<SavedObject>(json);
				}

				cache.Add((name, module), savedObject);
			}

			return savedObject.Deserialize<T>();
		}

		string SanetizeName(string name)
		{
			name = name.Replace(" ", "");

			return name;
		}
	}
}
