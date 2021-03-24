using AgileConfig.Server.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgileConfig.Server.Apisite
{
    public class Appsettings
    {
        /// 是否演示模式
        /// </summary>
        public static bool IsPreviewMode => "true".Equals(Global.Config["preview_mode"], StringComparison.CurrentCultureIgnoreCase);
        /// <summary>
        /// 是否控制台模式
        /// </summary>
        public static bool IsAdminConsoleMode => "true".Equals(Global.Config["adminConsole"], StringComparison.CurrentCultureIgnoreCase);
    }
}
