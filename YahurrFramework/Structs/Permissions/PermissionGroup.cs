using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;

namespace YahurrFramework.Structs
{
	internal class PermissionGroup
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

		public bool IsFiltered(ulong id, PermissionTarget target, out bool result)
		{
			bool filtered = false;
			bool found = false;

			for (int i = 0; i < Permissions.Count; i++)
			{
				Permission permission = Permissions[i];

				if (permission.Target == target && permission.IsFiltered(id, out bool r))
				{
					filtered = filtered || r;
					found = true;
				}
			}

			result = filtered;
			return found;
		}
	}
}
