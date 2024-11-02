using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace AgileConfig.Server.Common
{
    public static class Encrypt
    {
        private static readonly ThreadLocal<MD5> Md5Instance = new(MD5.Create);
        public static string Md5(string txt)
        {
            var inputBytes = Encoding.ASCII.GetBytes(txt);
            var hashBytes = Md5Instance.Value.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
