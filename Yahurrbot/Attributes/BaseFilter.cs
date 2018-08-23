using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class BaseFilter : Attribute
    {
		HashSet<long> IDs;
		FilterType type;

		public BaseFilter(FilterType type, params long[] IDs)
		{
			this.IDs = new HashSet<long>();
			this.type = type;

			for (int i = 0; i < IDs.Length; i++)
				this.IDs.Add(IDs[i]);
		}

		/// <summary>
		/// Check wether an server id is on the filter list.
		/// </summary>
		/// <param name="id">ID to check</param>
		/// <returns></returns>
		public bool IsFiltered(long id)
		{
			bool found = IDs.Contains(id);

			if (type == FilterType.Blacklist)
				found = !found;

			return found;
		}
    }
}
