﻿using System;
using System.Collections.Generic;
using System.Text;
using YFramework.Enums;

namespace YFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class ServerFilter : BaseFilter
	{
		public ServerFilter(FilterType type, params ulong[] serverIDs) : base(type, serverIDs)
		{
		}
	}
}
