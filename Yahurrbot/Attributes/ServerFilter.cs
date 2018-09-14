using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ServerFilter : BaseFilter
	{
		public ServerFilter(FilterType type, params ulong[] serverIDs) : base(type, serverIDs)
		{
		}
	}
}
