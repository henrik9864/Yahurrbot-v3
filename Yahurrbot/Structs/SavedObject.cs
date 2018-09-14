using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YahurrBot.Enums;
using YahurrFramework;
using YahurrFramework.Managers;

namespace YahurrBot.Structs
{
    internal class SavedObject
    {
		public string Name { get; private set; }

		public string Extension { get; private set; }

		[JsonIgnore]
		public Type Type { get; private set; }

		public string ModuleID { get; private set; }

		public string Path { get; private set; }

		[JsonProperty]
		string typeName;

		[JsonConstructor]
		private SavedObject(string Name, string Extension, string ModuleID, string typeName, string Path)
		{
			this.Name = Name;
			this.Type = ModuleManager.GetType(typeName);
			this.ModuleID = ModuleID;
			this.Extension = Extension;
			this.Path = Path;
			this.typeName = typeName;
		}

		public SavedObject(string name, string ex, YModule module, Type type) : this(name, ex, module.ID, type.FullName, $"Saves/{SanetizeName(module.Name)}/{name}{ex}")
		{
			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");
		}

		/// <summary>
		/// Deserialize Object JSON to type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="deserializer"></param>
		/// <returns></returns>
		public async Task<T> Deserialize<T>(Func<string, T> deserializer)
		{
			string json;
			using (StreamReader reader = new StreamReader(Path))
				json = await reader.ReadToEndAsync().ConfigureAwait(false);

			if (deserializer != null)
				return deserializer(json);
			else
			{
				SerializationType type = (SerializationType)Enum.Parse(typeof(SerializationType), Extension.Replace(".", ""), true);
				return Deserialize<T>(json, type);
			}
		}

		/// <summary>
		/// Validate if object is of type.
		/// </summary>
		/// <param name="type">Type to validate for</param>
		/// <returns></returns>
		public bool IsValid(Type type)
		{
			return Type.IsAssignableFrom(type);
		}

		static string SanetizeName(string name)
		{
			name = name.Replace(" ", "");

			return name;
		}

		T Deserialize<T>(string json, SerializationType type)
		{
			switch (type)
			{
				case SerializationType.JSON:
					return JsonConvert.DeserializeObject<T>(json);
				default:
					return default(T);
			}
		}
	}
}
