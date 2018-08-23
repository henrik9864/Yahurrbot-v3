using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	public class ChannelFilter : BaseFilter
	{
		public ChannelFilter(FilterType type, params long[] channelIDs) : base(type, channelIDs)
		{
		}
	}
}
