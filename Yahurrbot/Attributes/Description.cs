using System;
using System.Collections.Generic;
using System.Text;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Class)]
    public class Description : Attribute
    {
		public string Name { get; }

		public string Summary { get; }

		public Description(string name, string summary)
		{
			this.Name = name;
			this.Summary = summary;
		}
    }
}
