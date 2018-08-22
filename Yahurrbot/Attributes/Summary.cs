using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
    public class Summary : Attribute
    {
		public string Value { get; }

		public Summary(string summary)
		{
			this.Value = summary;
		}
    }
}
