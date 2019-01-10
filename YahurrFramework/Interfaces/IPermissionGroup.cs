using System;
using System.Collections.Generic;
using System.Text;
using YahurrFramework.Enums;
using YahurrFramework.Enums.Permissions;
using YahurrFramework.Structs;

namespace YahurrFramework.Interfaces
{
    interface IPermissionGroup
    {
        List<Permission> Permissions { get; }

        Dictionary<string, string> Properties { get; }

        PermissionStatus IsFiltered(ulong userID, PermissionTarget target);
    }
}
