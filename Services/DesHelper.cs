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
            byte[] keyBytes = GetKeyBytes(key);
            des.Key = keyBytes;
            des.IV = keyBytes;
            var bytes = Encoding.UTF8.GetBytes(data ?? string.Empty);
            return Convert.ToBase64String(des.CreateEncryptor().TransformFinalBlock(bytes, 0, bytes.Length));
        }

        public static string Decrypt(string data, string key)
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("请输入需要解密的内容");

            using var des = DES.Create();
            byte[] keyBytes = GetKeyBytes(key);
            des.Key = keyBytes;
            des.IV = keyBytes;
            var bytes = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(des.CreateDecryptor().TransformFinalBlock(bytes, 0, bytes.Length));
        }

        private static byte[] GetKeyBytes(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("请输入8字节DES密钥");

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            if (keyBytes.Length != 8)
                throw new ArgumentException($"DES密钥必须是8字节，当前：{keyBytes.Length}");

            return keyBytes;
        }
    }
}
