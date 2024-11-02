using System;
using System.ComponentModel;
using System.Linq;

namespace AgileConfig.Server.Common
{
    public static class EnumExt
    {
        public static string ToDesc(this Enum e)
        {
            var fi = e.GetType().GetField(e.ToString());
            if (fi == null)
            {
                return "";
            }
            var attrs = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrs.Length != 0)
            {
                return attrs.First().Description;
            }
            else
            {
                return e.ToString();
            }

        }
    }
}
