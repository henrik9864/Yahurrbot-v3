using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YahurrFramework.Enums;
using YahurrFramework;
using YahurrFramework.Managers;

namespace YahurrFramework.Structs
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
			this.Type = Type.GetType(typeName);
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
        /// Derserialize file with JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<T> Deserialize<T>()
        {
            string json;
            using (StreamReader reader = new StreamReader(Path))
                json = await reader.ReadToEndAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<T>(json);
        }

		/// <summary>
		/// Deserialize file with a custom serializer
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="deserializer"></param>
		/// <returns></returns>
		public async Task<T> Deserialize<T>(Func<string, T> deserializer)
		{
			string json;
			using (StreamReader reader = new StreamReader(Path))
				json = await reader.ReadToEndAsync().ConfigureAwait(false);

			return deserializer(json);
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
	}
}
