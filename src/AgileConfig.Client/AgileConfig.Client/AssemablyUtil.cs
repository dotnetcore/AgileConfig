using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Client
{
    internal class AssemablyUtil
    {
        public static string GetVer()
        {
            var type = typeof(AssemablyUtil);
            var ver = type.Assembly.GetName().Version;

            return $"{ver.Major}.{ver.Minor}.{ver.Build}";
        }
    }
}
