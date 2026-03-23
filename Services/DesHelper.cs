using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace EncryptTool.Services
{
    public static class DesHelper
    {
        public static string Encrypt(string data, string key)
        {
            using var des = DES.Create();
            des.Key = Encoding.UTF8.GetBytes(key.PadRight(8).Substring(0, 8));
            des.IV = Encoding.UTF8.GetBytes(key.PadRight(8).Substring(0, 8));
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
        }

        public static string Decrypt(string data, string key)
        {
            using var des = DES.Create();
            des.Key = Encoding.UTF8.GetBytes(key.PadRight(8).Substring(0, 8));
            des.IV = Encoding.UTF8.GetBytes(key.PadRight(8).Substring(0, 8));
            var bytes = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(des.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.Length));
        }
    }
}
