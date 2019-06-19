using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using YahurrFramework.Enums;
using YahurrFramework.Structs;

namespace YahurrFramework.Managers
{
	internal class FileManager : BaseManager
	{
		Dictionary<(string name, string moduleID), SavedObject> savedObjects;

		public FileManager(YahurrBot bot, DiscordSocketClient client) : base(bot, client)
		{
            savedObjects = new Dictionary<(string, string), SavedObject>();
			Directory.CreateDirectory("Saves");
			Directory.CreateDirectory("Files");
        }

		/// <summary>
		/// Sav object to file as json.
		/// </summary>
		/// <param name="obj">Object to save</param>
		/// <param name="name">Identefier for loading this object.</param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <returns></returns>
		public async Task Save(object obj, string name, YModule module, bool @override, bool append)
		{
			string json = JsonConvert.SerializeObject(obj);
            SavedObject savedObject = new SavedObject(name, ".json", module, obj.GetType());

			await SaveAsync(savedObject, json, @override, append).ConfigureAwait(false);
		}

		/// <summary>
		/// Save object to file as custom type.
		/// </summary>
		/// <param name="obj">Object to save.</param>
		/// <param name="name">Identefier for loading this object.</param>
		/// <param name="extension">File extension.</param>
		/// <param name="serializer">Custom method for serializing objects.</param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <returns></returns>
		public async Task Save(object obj, string name, string extension, Func<object, string> serializer, YModule module, bool @override, bool append)
		{
			string json = serializer(obj);
			SavedObject savedObject = new SavedObject(name, extension, module, obj.GetType());

			await SaveAsync(savedObject, json, @override, append).ConfigureAwait(false);
		}

		/// <summary>
		/// Load object from cache or file.
		/// </summary>
		/// <typeparam name="T">Class you have saved</typeparam>
		/// <param name="name">Identefier</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public async Task<T> Load<T>(string name, YModule module)
		{
			if (!savedObjects.TryGetValue((name, module.ID), out SavedObject savedObject))
				return default(T);

			return await savedObject.Deserialize<T>().ConfigureAwait(false);
		}

		/// <summary>
		/// Load object from cache or file.
		/// </summary>
		/// <typeparam name="T">Class you have saved</typeparam>
		/// <param name="name">Identefier</param>
		/// <param name="deserializer"></param>
		/// <param name="module"></param>
		/// <returns></returns>
		public async Task<T> Load<T>(string name, Func<string, T> deserializer, YModule module)
		{
			if (!savedObjects.TryGetValue((name, module.ID), out SavedObject savedObject))
				return default(T);

			return await savedObject.Deserialize(deserializer).ConfigureAwait(false);
		}

		/// <summary>
		/// Check if file exists
		/// </summary>
		/// <param name="name">Save identefier.</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public Task<bool> Exists(string name, YModule module)
		{
			return Task.Run(() => savedObjects.TryGetValue((name, module.ID), out SavedObject savedObject));
		}

		/// <summary>
		/// Check if saved item is of type.
		/// </summary>
		/// <param name="name">Save identefier.</param>
		/// <param name="type">Type to check for</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public bool IsValid(string name, Type type, YModule module)
		{
			if (savedObjects.TryGetValue((name, module.ID), out SavedObject savedObject))
			{
				return savedObject.IsValid(type);
			}

			return false;
		}

		async Task SaveAsync(SavedObject savedObject, string json, bool @override, bool append)
		{
			await WriteToFile(savedObject, json, @override, append).ConfigureAwait(false);
			AddSavedObject((savedObject.Name, savedObject.ModuleID), savedObject, @override);
			await SaveObjectList();
		}

		/// <summary>
		/// Write to file, override if its already there
		/// </summary>
		/// <param name="name"></param>
		/// <param name="savedObject"></param>
		/// <param name="toWrite"></param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <param name="append"></param>
		/// <returns></returns>
		async Task WriteToFile(SavedObject savedObject, string toWrite, bool @override, bool append)
		{
			string path = savedObject.Path;

			if (@override || append)
			{
				using (FileStream fileStream = new FileStream(path, append ? FileMode.Append : FileMode.OpenOrCreate, FileAccess.Write))
				{
					if (@override && !append)
						fileStream.SetLength(0);

					using (StreamWriter writer = new StreamWriter(fileStream))
					{
						await writer.WriteAsync(toWrite).ConfigureAwait(false);
					}
				}
			}
			else if (!File.Exists(path))
			{
				using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
				{
					using (StreamWriter writer = new StreamWriter(fileStream))
						await writer.WriteAsync(toWrite).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Save list of all stored objects.
		/// </summary>
		async Task SaveObjectList()
		{
			string json = JsonConvert.SerializeObject(savedObjects.Values, Formatting.Indented);
			string path = "Saves/SavedObjects.json";

			using (FileStream fileStream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write))
			{
				fileStream.SetLength(0);

				using (StreamWriter writer = new StreamWriter(fileStream))
					await writer.WriteAsync(json).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Load dictionary of all saved objects.
		/// </summary>
		internal async Task LoadObjectList()
		{
			string path = "Saves/SavedObjects.json";
			string json;

			if (File.Exists(path))
			{
				using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				using (StreamReader reader = new StreamReader(fileStream))
					json = await reader.ReadToEndAsync().ConfigureAwait(false);

				List<SavedObject> objects = JsonConvert.DeserializeObject<List<SavedObject>>(json);
				savedObjects = objects.ToDictionary(a => (a.Name, a.ModuleID));
			}
			else
			{
				await SaveObjectList();
			}
		}

		/// <summary>
		/// Add object to cache, ovveride if its already there
		/// </summary>
		/// <param name="key"></param>
		/// <param name="savedObject"></param>
		/// <param name="override"></param>
		void AddSavedObject((string name, string moduleID) key, SavedObject savedObject, bool @override)
		{
			if (savedObjects.ContainsKey(key))
			{
				if (@override)
					savedObjects[key] = savedObject;
			}
			else
				savedObjects.Add(key, savedObject);
		}
	}
}
