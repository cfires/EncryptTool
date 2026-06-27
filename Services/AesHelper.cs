using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EncryptTool.Services
{
    public static class AesHelper
    {
        private static readonly byte[] OpenSslPrefix = Encoding.ASCII.GetBytes("Salted__");

        /// <summary>
        /// AES 加密，支持 CBC/ECB、PKCS7/ZeroPadding 和不同字符集。
        /// </summary>
        public static string Encrypt(
            string data,
            string key,
            string mode = "CBC",
            string padding = "PKCS7",
            string charset = "UTF-8")
        {
            Encoding enc = GetEncoding(charset);
            using var aes = CreateAes(key, mode, padding, enc);

            byte[] dataBytes = enc.GetBytes(data ?? string.Empty);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encrypted = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// AES 解密。
        /// </summary>
        public static string Decrypt(
            string data,
            string key,
            string mode = "CBC",
            string padding = "PKCS7",
            string charset = "UTF-8")
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("请输入需要解密的Base64内容");

            Encoding enc = GetEncoding(charset);
            using var aes = CreateAes(key, mode, padding, enc);

            byte[] bytes = Convert.FromBase64String(data);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] decrypted = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

            string result = enc.GetString(decrypted);
            return aes.Padding == PaddingMode.Zeros ? result.TrimEnd('\0') : result;
        }

        #region OpenSSL 兼容模式
        public static string EncryptOpenSsl(string data, string key, string charset = "UTF-8")
        {
            Encoding enc = GetEncoding(charset);
            byte[] passwordBytes = Encoding.ASCII.GetBytes(key ?? string.Empty);
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
            byte[] dataBytes = enc.GetBytes(data ?? string.Empty);
            cs.Write(dataBytes, 0, dataBytes.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptOpenSsl(string base64Data, string key, string charset = "UTF-8")
        {
            if (string.IsNullOrWhiteSpace(base64Data))
                throw new ArgumentException("请输入需要解密的OpenSSL密文");

            Encoding enc = GetEncoding(charset);
            byte[] allBytes = Convert.FromBase64String(base64Data);

            if (allBytes.Length < 16)
                throw new InvalidOperationException("无效的OpenSSL格式密文");

            byte[] prefix = allBytes.AsSpan(0, 8).ToArray();
            if (!CryptographicOperations.FixedTimeEquals(prefix, OpenSslPrefix))
                throw new InvalidOperationException("密文格式不正确，非Salted__开头");

            byte[] salt = allBytes.AsSpan(8, 8).ToArray();
            byte[] cipherBytes = allBytes.AsSpan(16).ToArray();

            DeriveKeyAndIv(Encoding.ASCII.GetBytes(key ?? string.Empty), salt, out byte[] aesKey, out byte[] iv);

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
            int targetLength = key.Length + iv.Length;
            byte[] keyAndIv = new byte[targetLength];

            while (total < targetLength)
            {
                data = md5.ComputeHash(Combine(data, password, salt));
                int copy = Math.Min(data.Length, targetLength - total);
                Buffer.BlockCopy(data, 0, keyAndIv, total, copy);
                total += copy;
            }

            Buffer.BlockCopy(keyAndIv, 0, key, 0, key.Length);
            Buffer.BlockCopy(keyAndIv, key.Length, iv, 0, iv.Length);
        }
        #endregion

        private static Aes CreateAes(string key, string mode, string padding, Encoding enc)
        {
            byte[] keyBytes = enc.GetBytes(key ?? string.Empty);
            if (keyBytes.Length is not 16 and not 24 and not 32)
                throw new ArgumentException($"AES密钥必须是16/24/32字节，当前：{keyBytes.Length}");

            var aes = Aes.Create();
            aes.Mode = mode == "ECB" ? CipherMode.ECB : CipherMode.CBC;
            aes.Padding = padding == "ZeroPadding" ? PaddingMode.Zeros : PaddingMode.PKCS7;
            aes.Key = keyBytes;

            if (aes.Mode == CipherMode.CBC)
            {
                byte[] ivBytes = new byte[16];
                Array.Copy(keyBytes, ivBytes, Math.Min(keyBytes.Length, ivBytes.Length));
                aes.IV = ivBytes;
            }

            return aes;
        }

        private static Encoding GetEncoding(string charset)
        {
            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
            {
                throw new ArgumentException($"不支持的字符集：{charset}", ex);
            }
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            using var ms = new MemoryStream();
            foreach (var array in arrays)
            {
                ms.Write(array, 0, array.Length);
            }

            return ms.ToArray();
        }
    }
}
