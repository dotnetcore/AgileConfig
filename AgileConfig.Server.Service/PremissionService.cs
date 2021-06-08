using AgileConfig.Server.Data.Entity;
using AgileConfig.Server.IService;
using System.Collections.Generic;

namespace AgileConfig.Server.Service
{
    public class PremissionService : IPremissionService
    {
        public string EditConfigPermissionKey => "EDIT_CONFIG";

        public string PublishConfigPermissionKey => "PUBLISH_CONFIG";
    }
}
