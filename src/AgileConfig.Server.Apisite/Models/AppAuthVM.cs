﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AgileConfig.Server.Apisite.Models
{
    [ExcludeFromCodeCoverage]
    public class AppAuthVM: IAppIdModel
    {
        public List<string> EditConfigPermissionUsers { get; set; }

        public List<string> PublishConfigPermissionUsers { get; set; }

        public string AppId { get; set; }
    }
}
