using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;
using YahurrFramework.Enums.Permissions;

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

		public PermissionStatus IsFiltered(ulong id)
		{
			PermissionStatus status = PermissionStatus.NotFound;

			if (Type == PermissionType.Whitelist)
				status = PermissionStatus.Denied;

			for (int i = 0; i < Identifiers.Count; i++)
			{
				ulong saved = Identifiers[i];

				if (saved == id)
					return Type == PermissionType.Whitelist ? PermissionStatus.Approved : PermissionStatus.Denied;
			}

			return status;
		}
	}
}
