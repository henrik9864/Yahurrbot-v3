using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
    public abstract class BaseFilter : Attribute
    {
		protected HashSet<ulong> IDs { get; }

		protected FilterType Type { get; }

		public BaseFilter(FilterType type, params ulong[] IDs)
		{
			this.IDs = new HashSet<ulong>();
			this.Type = type;

			for (int i = 0; i < IDs.Length; i++)
				this.IDs.Add(IDs[i]);
		}

		/// <summary>
		/// Check wether an server id is on the filter list.
		/// </summary>
		/// <param name="id">ID to check</param>
		/// <returns></returns>
		public bool IsFiltered(ulong id)
		{
			bool found = IDs.Contains(id);

			if (Type == FilterType.Blacklist)
				found = !found;

			return found;
		}
    }
}
