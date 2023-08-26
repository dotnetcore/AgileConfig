using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

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
            if (attrs != null & attrs.Any())
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
