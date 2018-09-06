using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YahurrFramework;

namespace YahurrBot.Structs
{
    internal class SavedObject
    {
		public string Name { get; private set; }

		public string Extension { get; private set; }

		public Type Type { get; private set; }

		public Module Module { get; private set; }

		public string Path { get; private set; }

		[JsonConstructor]
		public SavedObject(string Name, string Ex, Module Module, Type Type)
		{
			this.Name = Name;
			this.Type = Type;
			this.Module = Module;
			this.Extension = Ex;
			this.Path = $"Saves/{Module.Name}/{Name}{Ex}";
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
				throw new Exception($"{Name} is saved as {Type.Name} not {generic.Name}");

			using (StreamReader reader = new StreamReader(Path))
			{
				string json = await reader.ReadToEndAsync().ConfigureAwait(false);
				JSchemaGenerator generator = new JSchemaGenerator();
				JSchema schema = generator.Generate(typeof(T));
				JToken token = JToken.Parse(json);

				if (token.IsValid(schema))
				{
					if (deserializer != null)
						return deserializer(json);
					else
						return token.ToObject<T>();
				}
				else
					throw new Exception($"Invalid JSON in file.");
			}
		}

		/// <summary>
		/// Validate if object is of type.
		/// </summary>
		/// <param name="type">Type to validate for</param>
		/// <returns></returns>
		public async Task<bool> IsValid(Type type)
		{
			using (StreamReader reader = new StreamReader(Path))
			{
				string json = await reader.ReadToEndAsync().ConfigureAwait(false);
				JSchemaGenerator generator = new JSchemaGenerator();
				JSchema schema = generator.Generate(type);
				JToken token = JToken.Parse(json);

				return token.IsValid(schema);
			}
		}
    }
}
