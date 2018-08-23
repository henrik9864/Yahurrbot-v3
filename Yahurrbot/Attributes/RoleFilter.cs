using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	public class RoleFilter : BaseFilter
	{
		public RoleFilter(FilterType type, params long[] roleIDs) : base(type, roleIDs)
		{
		}
	}
}
