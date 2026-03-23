using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EncryptTool.Services
{
    public static class AesHelper
    {
        /// <summary>
        /// AES 加密（自动支持 16/24/32 位密钥）& (支持模式/填充/编码)
        /// </summary>
        public static string Encrypt(
            string data,
            string key,
            string mode = "CBC",
            string padding = "PKCS7",
            string charset = "UTF-8")
        {
            using var aes = Aes.Create();

            // 1. 设置模式
            aes.Mode = mode == "ECB" ? CipherMode.ECB : CipherMode.CBC;

            // 2. 设置填充
            aes.Padding = padding == "ZeroPadding"
                ? PaddingMode.Zeros
                : PaddingMode.PKCS7;

            // 3. 获取编码
            Encoding enc = Encoding.GetEncoding(charset);

            // 4. 密钥
            byte[] keyBytes = enc.GetBytes(key);
            if (keyBytes.Length is not 16 and not 24 and not 32)
                throw new ArgumentException($"密钥必须是16/24/32位，当前：{keyBytes.Length}");

            aes.Key = keyBytes;

            // 5. IV（ECB 模式不用）
            if (aes.Mode == CipherMode.CBC)
            {
                byte[] ivBytes = new byte[16];
                Array.Copy(keyBytes, ivBytes, Math.Min(keyBytes.Length, 16));
                aes.IV = ivBytes;
            }

            // 6. 加密
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] dataBytes = enc.GetBytes(data);
            byte[] encrypted = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// AES 解密
        /// </summary>
        public static string Decrypt(
            string data,
            string key,
            string mode = "CBC",
            string padding = "PKCS7",
            string charset = "UTF-8")
        {
            using var aes = Aes.Create();
            aes.Mode = mode == "ECB" ? CipherMode.ECB : CipherMode.CBC;
            aes.Padding = padding == "ZeroPadding"
                ? PaddingMode.Zeros
                : PaddingMode.PKCS7;

            Encoding enc = Encoding.GetEncoding(charset);
            byte[] keyBytes = enc.GetBytes(key);
            aes.Key = keyBytes;

            if (aes.Mode == CipherMode.CBC)
            {
                byte[] ivBytes = new byte[16];
                Array.Copy(keyBytes, ivBytes, Math.Min(keyBytes.Length, 16));
                aes.IV = ivBytes;
            }

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] bytes = Convert.FromBase64String(data);
            byte[] decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            string result = enc.GetString(decrypted);

            // 零填充需要去掉末尾 \0
            if (aes.Padding == PaddingMode.Zeros)
                result = result.TrimEnd('\0');

            return result;
        }

        #region OpenSSL 兼容模式
        private static readonly byte[] OpenSslPrefix = Encoding.ASCII.GetBytes("Salted__");

        public static string EncryptOpenSsl(string data, string key, string charset = "UTF-8")
        {
            Encoding enc = Encoding.GetEncoding(charset);
            byte[] passwordBytes = Encoding.ASCII.GetBytes(key);
            byte[] salt = new byte[8];
            RandomNumberGenerator.Fill(salt);

            DeriveKeyAndIv(passwordBytes, salt, out byte[] aesKey, out byte[] iv);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            ms.Write(OpenSslPrefix, 0, OpenSslPrefix.Length);
            ms.Write(salt, 0, salt.Length);

            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            byte[] dataBytes = enc.GetBytes(data);
            cs.Write(dataBytes, 0, dataBytes.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptOpenSsl(string base64Data, string key, string charset = "UTF-8")
        {
            Encoding enc = Encoding.GetEncoding(charset);
            byte[] allBytes = Convert.FromBase64String(base64Data);

            if (allBytes.Length < 16)
                throw new InvalidOperationException("无效的OpenSSL格式密文");

            byte[] prefix = allBytes.AsSpan(0, 8).ToArray();
            if (!CompareBytes(prefix, OpenSslPrefix))
                throw new InvalidOperationException("密文格式不正确，非Salted__开头");

            byte[] salt = allBytes.AsSpan(8, 8).ToArray();
            byte[] cipherBytes = allBytes.AsSpan(16).ToArray();

            DeriveKeyAndIv(Encoding.ASCII.GetBytes(key), salt, out byte[] aesKey, out byte[] iv);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherBytes, 0, cipherBytes.Length);
            cs.FlushFinalBlock();

            return enc.GetString(ms.ToArray());
        }

        private static void DeriveKeyAndIv(byte[] password, byte[] salt, out byte[] key, out byte[] iv)
        {
            key = new byte[32];
            iv = new byte[16];
            using var md5 = MD5.Create();

            byte[] data = Array.Empty<byte>();
            int total = 0;

            while (total < key.Length + iv.Length)
            {
                byte[] temp = Combine(data, password, salt);
                data = md5.ComputeHash(temp);

                int copy = Math.Min(data.Length, key.Length + iv.Length - total);
                Buffer.BlockCopy(data, 0, total < key.Length ? key : iv,
                                total < key.Length ? total : total - key.Length, copy);
                total += copy;
            }
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            using var ms = new MemoryStream();
            foreach (var a in arrays) ms.Write(a, 0, a.Length);
            return ms.ToArray();
        }

        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
        #endregion
    }

}
