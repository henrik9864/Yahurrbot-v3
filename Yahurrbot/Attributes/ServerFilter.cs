﻿using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	public class ServerFilter : BaseFilter
	{
		public ServerFilter(FilterType type, params long[] serverIDs) : base(type, serverIDs)
		{
		}
	}
}
