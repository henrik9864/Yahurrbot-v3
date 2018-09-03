using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Attributes
{

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class)]
	public class Name : Attribute
	{
		public string Value { get; }

		public Name(string summary)
		{
			this.Value = summary;
		}
	}
}
