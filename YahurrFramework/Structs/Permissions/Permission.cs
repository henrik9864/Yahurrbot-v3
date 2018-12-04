using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Structs
{
	internal class Permission
	{
		public PermissionType Type { get; private set; }

		public PermissionTarget Target { get; private set; }

		public List<ulong> Identifiers { get; private set; }

		public Permission(PermissionType type, PermissionTarget target, List<ulong> identifiers)
		{
			Type = type;
			Target = target;
			Identifiers = identifiers;
		}

		public bool IsFiltered(ulong id, out bool result)
		{
			bool filtered = Type == PermissionType.Whitelist ? true : false;

			for (int i = 0; i < Identifiers.Count; i++)
			{
				ulong saved = Identifiers[i];

				if (saved == id){
					result = !filtered;
					return true;
				}
			}

			result = filtered;
			return Type == PermissionType.Whitelist ? true : false;
		}
	}
}
