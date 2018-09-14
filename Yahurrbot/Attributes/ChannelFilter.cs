using System;
using System.Collections.Generic;
using System.Text;
using YFramework.Enums;

namespace YFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class ChannelFilter : BaseFilter
	{
		public ChannelFilter(FilterType type, params ulong[] channelIDs) : base(type, channelIDs)
		{
		}
	}
}
