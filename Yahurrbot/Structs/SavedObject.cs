using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YahurrBot.Enums;
using YahurrFramework;

namespace YahurrBot.Structs
{
    internal class SavedObject
    {
		public string Name { get; private set; }

		public string Extension { get; private set; }

		public Type Type { get; private set; }

		public string ModuleID { get; private set; }

		public string Path { get; private set; }

		[JsonIgnore]
		string typeName;

		[JsonConstructor]
		private SavedObject(string Name, string Extension, string ModuleID, Type Type, string Path)
		{
			this.Name = Name;
			this.Type = Type;
			this.ModuleID = ModuleID;
			this.Extension = Extension;
			this.Path = Path;
			this.typeName = Type.Name;
		}

		public SavedObject(string name, string ex, Module module, Type type) : this(name, ex, module.ID, type, $"Saves/{SanetizeName(module.Name)}/{name}{ex}")
		{
			DirectoryInfo dir = Directory.CreateDirectory($"Saves/{SanetizeName(module.Name)}");
		}

		/// <summary>
		/// Deserialize Object JSON to type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public async Task<T> Deserialize<T>(Func<string, T> deserializer)
		{
			Type generic = typeof(T);
			if (!generic.IsAssignableFrom(Type))
				throw new Exception($"{Name} is saved as {Type?.Name} not {generic?.Name}");

			string json;
			using (StreamReader reader = new StreamReader(Path))
			{
				json = await reader.ReadToEndAsync().ConfigureAwait(false);
			}

			SerializationType type = (SerializationType)Enum.Parse(typeof(SerializationType), Extension.Replace(".", ""), true);

			if (deserializer != null)
				return deserializer(json);
			else
				return Deserialize<T>(json, type);
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
				case SerializationType.JSV:
					return TypeSerializer.DeserializeFromString<T>(json);
				case SerializationType.CSV:
					return CsvSerializer.DeserializeFromString<T>(json);
				default:
					return default(T);
			}
		}
	}
}
