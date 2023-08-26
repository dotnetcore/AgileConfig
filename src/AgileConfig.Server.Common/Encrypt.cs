using System;
using System.Security.Cryptography;
using System.Text;

namespace AgileConfig.Server.Common
{
    public static class Encrypt
    {
        private static readonly MD5 Md5Instance = MD5.Create();
        public static string Md5(string txt)
        {
            var inputBytes = Encoding.ASCII.GetBytes(txt);
            var hashBytes = Md5Instance.ComputeHash(inputBytes);
            return Convert.ToHexString(hashBytes);
        }
    }
}
