using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;
using YahurrFramework.Enums.Permissions;
using YahurrFramework.Interfaces;

namespace YahurrFramework.Structs
{
	internal class PermissionGroup : IPermissionGroup
    {
		public PermissionClass Class { get; private set; }

		public string Name { get; private set; }

		public PermissionGroupType Type { get; private set; }

		public List<Permission> Permissions { get; private set; }

		public Dictionary<string, string> Properties { get; private set; }

		public PermissionGroup(PermissionClass @class, string name, PermissionGroupType type, Dictionary<string, string> properties)
		{
			Class = @class;
			Name = name;
			Type = type;
			Permissions = new List<Permission>();
			Properties = properties;
		}

		public void AddPermission(Permission permission)
		{
			Permissions.Add(permission);
		}

		public PermissionStatus IsFiltered(ulong id, PermissionTarget target)
		{
			for (int i = 0; i < Permissions.Count; i++)
			{
				Permission permission = Permissions[i];

				if (permission.Target == target)
				{
					PermissionStatus targetStatus = permission.IsFiltered(id);

					if (targetStatus != PermissionStatus.NotFound)
						return targetStatus;
				}
			}

			return PermissionStatus.NotFound;
		}
	}
}
