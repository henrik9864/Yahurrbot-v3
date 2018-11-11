using System;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class Example : Attribute
	{
		public string Value { get; }

		public Example(string example)
		{
			Value = example;
		}
	}
}
