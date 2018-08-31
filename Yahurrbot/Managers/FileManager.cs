using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;
using ServiceStack.Text;
using YahurrBot.Enums;
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
		/// Sav object to file as json.
		/// </summary>
		/// <param name="obj">Object to save</param>
		/// <param name="name">Identefier for loading this object.</param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <returns></returns>
		public async Task Save(object obj, string name, YahurrModule module, bool @override, bool append)
		{
			string json = Serialize(obj, SerializationType.JSON);
			SavedObject savedObject = new SavedObject(name, ".json", module, obj.GetType());

			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");

			await WriteToFile(savedObject, json, @override, append).ConfigureAwait(false);
			AddToCache((name, module), savedObject, @override);
			SaveObjectList();
		}

		/// <summary>
		/// Save object to file as type.
		/// </summary>
		/// <param name="obj">Object to save.</param>
		/// <param name="name">Identefier for loading this object.</param>
		/// <param name="type">Type to save as.</param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <returns></returns>
		public async Task Save(object obj, string name, SerializationType type, YahurrModule module, bool @override, bool append)
		{
			string json = Serialize(obj, type);
			SavedObject savedObject = new SavedObject(name, $".{type.ToString()}", module, obj.GetType());

			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");

			await WriteToFile(savedObject, json, @override, append).ConfigureAwait(false);
			AddToCache((name, module), savedObject, @override);
			SaveObjectList();
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
		public async Task Save(object obj, string name, string extension, Func<object, string> serializer, YahurrModule module, bool @override, bool append)
		{
			string json = serializer(obj);
			SavedObject savedObject = new SavedObject(name, extension, module, obj.GetType());

			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");

			await WriteToFile(savedObject, json, @override, append).ConfigureAwait(false);
			AddToCache((name, module), savedObject, @override);
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
				return default(T);

			return await savedObject.Deserialize<T>().ConfigureAwait(false);
		}

		/// <summary>
		/// Check if file exists
		/// </summary>
		/// <param name="name">Save identefier.</param>
		/// <param name="module"></param>
		/// <returns></returns>
		public Task<bool> Exists(string name, YahurrModule module)
		{
			return Task.Run(() => savedObjects.TryGetValue((name, module), out SavedObject savedObject));
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
			if (savedObjects.TryGetValue((name, module), out SavedObject savedObject))
			{
				return await savedObject.IsValid(type).ConfigureAwait(false);
			}

			return false;
		}

		/// <summary>
		/// Write to file, override if its already there
		/// </summary>
		/// <param name="name"></param>
		/// <param name="toWrite"></param>
		/// <param name="module"></param>
		/// <param name="override"></param>
		/// <returns></returns>
		async Task WriteToFile(SavedObject savedObject, string toWrite, bool @override, bool append)
		{
			string path = GetPath(savedObject);

			if (!File.Exists(path))
			{
				using (StreamWriter writer = File.CreateText(path))
					await writer.WriteAsync(toWrite).ConfigureAwait(false);
			}
			else if (@override || append)
			{
				using (StreamWriter writer = new StreamWriter(path, append))
					await writer.WriteAsync(toWrite).ConfigureAwait(false);
			}
		}

		void SaveObjectList()
		{
			string json = JsonConvert.SerializeObject(savedObjects.Values);
			File.WriteAllText("Saves/SavedObjects.json", json);
		}

		void LoadObjectList()
		{
			if (File.Exists("Saves/SavedObjects.json"))
			{
				string json = File.ReadAllText("Saves/SavedObjects.json");
				List<SavedObject> objects = JsonConvert.DeserializeObject<List<SavedObject>>(json);

				savedObjects = objects.ToDictionary(a => (a.Name, a.Module));
			}
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
		/// <param name="ex"></param>
		/// <param name="module"></param>
		/// <returns></returns>
		string GetPath(SavedObject savedObject)
		{
			return $"Saves/{SanetizeName(savedObject.Module.Name)}/{savedObject.Name}{savedObject.Extension}";
		}

		/// <summary>
		/// Add object to cache, ovveride if its already there
		/// </summary>
		/// <param name="key"></param>
		/// <param name="savedObject"></param>
		/// <param name="override"></param>
		void AddToCache((string name, YahurrModule module) key, SavedObject savedObject, bool @override)
		{
			if (savedObjects.TryGetValue(key, out SavedObject so))
			{
				if (@override)
				{
					savedObjects[key] = savedObject;
				}
}
			else
				savedObjects.Add(key, savedObject);
		}

		/// <summary>
		/// Convert any object to string useing serialization type.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		string Serialize(object obj, SerializationType type)
		{
			switch (type)
			{
				case SerializationType.JSON:
					return JsonConvert.SerializeObject(obj);
				case SerializationType.JSV:
					return TypeSerializer.SerializeToString(obj, obj.GetType());
				case SerializationType.CSV:
					return CsvSerializer.SerializeToString(obj);
				default:
					return null;
			}
		}
	}
}
