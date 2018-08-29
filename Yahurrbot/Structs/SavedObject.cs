using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrBot.Structs
{
    internal class SavedObject
    {
		public string Name { get; private set; }

		public Type Type { get; private set; }

		public string Object { get; private set; }

		[JsonConstructor]
		private SavedObject(string Name, Type Type, string Object)
		{
			this.Name = Name;
			this.Type = Type;
			this.Object = Object;
		}

		public SavedObject(string Name, object Object)
		{
			this.Name = Name;
			this.Type = Object.GetType();
			this.Object = JsonConvert.SerializeObject(Object);
		}

		/// <summary>
		/// Deserialize Object JSON to type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T Deserialize<T>()
		{
			Type generic = typeof(T);
			if (!generic.IsAssignableFrom(Type))
				throw new Exception($"{Name} is saved as {Type.Name} not {generic.Name}");

			JSchemaGenerator generator = new JSchemaGenerator();
			JSchema schema = generator.Generate(typeof(T));
			JToken token = JToken.Parse(Object);

			if (token.IsValid(schema))
				return token.ToObject<T>();
			else
				throw new Exception($"Invalid JSON in file.");
		}

		/// <summary>
		/// Validate if object is of type.
		/// </summary>
		/// <param name="type">Type to validate for</param>
		/// <returns></returns>
		public bool IsValid(Type type)
		{
			JSchemaGenerator generator = new JSchemaGenerator();
			JSchema schema = generator.Generate(type);
			JToken token = JToken.Parse(Object);

			return token.IsValid(schema);
		}
    }
}
