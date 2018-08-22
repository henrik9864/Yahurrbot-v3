using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Attributes
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ServerFilter : Attribute
    {
		HashSet<long> serverIDs;
		FilterType type;

		public ServerFilter(FilterType type, params long[] serverID)
		{
			serverIDs = new HashSet<long>();
			this.type = type;

			for (int i = 0; i < serverID.Length; i++)
				serverIDs.Add(serverID[i]);
		}

		/// <summary>
		/// Check wether an server id is on the filter list.
		/// </summary>
		/// <param name="id">ID to check</param>
		/// <returns></returns>
		public bool IsFiltered(long id)
		{
			bool found = serverIDs.Contains(id);

			if (type == FilterType.Blacklist)
				found = !found;

			return found;
		}
    }
}
