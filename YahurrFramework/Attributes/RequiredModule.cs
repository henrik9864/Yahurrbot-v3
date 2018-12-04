using System;
using System.Collections.Generic;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class RequiredModule : Attribute
	{
		public List<Type> Types;

		public RequiredModule(params Type[] types)
		{
			Types = new List<Type>();
			Types.AddRange(types);
		}
	}
}
