using System;
using System.Collections.Generic;
using System.Text;
using YFramework.Enums;

namespace YFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class RoleFilter : BaseFilter
	{
		public RoleFilter(FilterType type, params ulong[] roleIDs) : base(type, roleIDs)
		{
		}
	}
}
