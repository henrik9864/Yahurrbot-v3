using Newtonsoft.Json;
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
				throw new Exception($"Type {generic.Name} is not assignable from {Type}");

			return JsonConvert.DeserializeObject<T>(Object);
		}
    }
}
