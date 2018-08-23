﻿using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class RoleFilter : BaseFilter
	{
		public RoleFilter(FilterType type, params ulong[] roleIDs) : base(type, roleIDs)
		{
		}
	}
}
