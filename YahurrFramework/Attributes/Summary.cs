using System;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class Summary : Attribute
	{
		public string Value { get; }

		public Summary(string summary)
		{
			this.Value = summary;
		}
	}
}
