using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using YahurrBot.Structs;

namespace YahurrFramework.Managers
{
	internal class FileManager : BaseManager
	{
		Dictionary<(string name, YahurrModule module), SavedObject> savedObjects;

		public FileManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
			savedObjects = new Dictionary<(string name, YahurrModule module), SavedObject>();
			Directory.CreateDirectory("Saves");
			LoadObjectList();
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
			string path = GetPath(name, module);
			string json = JsonConvert.SerializeObject(obj);
			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");
			SavedObject savedObject = new SavedObject(name, module, obj.GetType());

			if (!await Exists(name, module).ConfigureAwait(false))
			{
				using (StreamWriter writer = File.CreateText(path))
					await writer.WriteAsync(json).ConfigureAwait(false);
			}
			else if (@override)
			{
				using (StreamWriter writer = new StreamWriter(path))
					await writer.WriteAsync(json).ConfigureAwait(false);
			}

			if (savedObjects.TryGetValue((name, module), out SavedObject so))
			{
				if (@override)
				{
					savedObjects[(name, module)] = savedObject;
				}
			}
			else
				savedObjects.Add((name, module), savedObject);

			SaveObjectList();
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
			if (!savedObjects.TryGetValue((name, module), out SavedObject savedObject))
			{
				savedObject = await GetSavedObject(name, module).ConfigureAwait(false);

				savedObjects.Add((name, module), savedObject);
			}

			return await savedObject.Deserialize<T>();
		}

		/// <summary>
		/// Check if save exists.
		/// </summary>
		/// <param name="name">Save identefier.</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public Task<bool> Exists(string name, YahurrModule module)
		{
			string path = GetPath(name, module);

			return Task.Run(() => File.Exists(path));
		}

		/// <summary>
		/// Check if saved item is of type.
		/// </summary>
		/// <param name="name">Save identefier.</param>
		/// <param name="type">Type to check for</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public async Task<bool> IsValid(string name, Type type, YahurrModule module)
		{
			if (await Exists(name, module).ConfigureAwait(false))
			{
				if (!savedObjects.TryGetValue((name, module), out SavedObject savedObject))
					savedObject = await GetSavedObject(name, module).ConfigureAwait(false);

				return savedObject.IsValid(type);
			}

			return false;
		}

		void SaveObjectList()
		{
			Console.WriteLine("hei");

			string json = JsonConvert.SerializeObject(savedObjects.Values);
			File.WriteAllText("Saves/SavedObjects.json", json);
		}

		void LoadObjectList()
		{
			string json = File.ReadAllText("Saves/SavedObjects.json");
			List<SavedObject> objects = JsonConvert.DeserializeObject<List<SavedObject>>(json);

			savedObjects = objects.ToDictionary(a => (a.Name, a.Module));
		}

		/// <summary>
		/// Removes spaces from string.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		string SanetizeName(string name)
		{
			name = name.Replace(" ", "");

			return name;
		}

		/// <summary>
		/// Get path from name and module.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="module"></param>
		/// <returns></returns>
		string GetPath(string name, YahurrModule module)
		{
			return $"Saves/{SanetizeName(module.Name)}/{name}.json";
		}

		async Task<SavedObject> GetSavedObject(string name, YahurrModule module)
		{
			string path = GetPath(name, module);
			using (StreamReader reader = new StreamReader(path))
			{
				string json = await reader.ReadToEndAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<SavedObject>(json);
			}
		}
	}
}
