using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Text;

namespace EncryptTool.Services
{
    public static class Sm4Helper
    {
        /// <summary>
        /// SM4 加密（CBC/ECB）
        /// </summary>
        /// <param name="data">明文</param>
        /// <param name="key">32位十六进制密钥</param>
        /// <param name="iv">32位十六进制IV（CBC使用）</param>
        public static string Encrypt(
            string data,
            string key,
            string iv = "",
            string mode = "CBC",
            string charset = "UTF-8",
            string outputFormat = "Base64")
        {
            Encoding enc = Encoding.GetEncoding(charset);

            // 密钥：32位十六进制 → 16字节
            byte[] keyBytes = HexStringToBytes(key);

            IBlockCipher engine = new SM4Engine();
            IBufferedCipher cipher;

            if (mode == "CBC")
            {
                byte[] ivBytes = HexStringToBytes(iv);
                var cbc = new CbcBlockCipher(engine);
                cipher = new PaddedBufferedBlockCipher(cbc, new Pkcs7Padding());
                cipher.Init(true, new ParametersWithIV(new KeyParameter(keyBytes), ivBytes));
            }
            else
            {
                var ecb = new EcbBlockCipher(engine);
                cipher = new PaddedBufferedBlockCipher(ecb, new Pkcs7Padding());
                cipher.Init(true, new KeyParameter(keyBytes));
            }

            byte[] dataBytes = enc.GetBytes(data);
            byte[] encrypted = cipher.DoFinal(dataBytes);

            return outputFormat == "Base64"
                ? Convert.ToBase64String(encrypted)
                : Convert.ToHexString(encrypted).ToLower();
        }

        /// <summary>
        /// SM4 解密
        /// </summary>
        /// <param name="data">密文（Base64 / HEX）</param>
        /// <param name="key">32位十六进制密钥</param>
        /// <param name="iv">32位十六进制IV（CBC使用）</param>
        public static string Decrypt(
            string data,
            string key,
            string iv = "",
            string mode = "CBC",
            string charset = "UTF-8",
            string inputFormat = "Base64")
        {
            Encoding enc = Encoding.GetEncoding(charset);
            byte[] keyBytes = HexStringToBytes(key);

            IBlockCipher engine = new SM4Engine();
            IBufferedCipher cipher;

            if (mode == "CBC")
            {
                byte[] ivBytes = HexStringToBytes(iv);
                var cbc = new CbcBlockCipher(engine);
                cipher = new PaddedBufferedBlockCipher(cbc, new Pkcs7Padding());
                cipher.Init(false, new ParametersWithIV(new KeyParameter(keyBytes), ivBytes));
            }
            else
            {
                var ecb = new EcbBlockCipher(engine);
                cipher = new PaddedBufferedBlockCipher(ecb, new Pkcs7Padding());
                cipher.Init(false, new KeyParameter(keyBytes));
            }

            byte[] dataBytes = inputFormat == "Base64"
                ? Convert.FromBase64String(data)
                : Convert.FromHexString(data);

            byte[] decrypted = cipher.DoFinal(dataBytes);
            return enc.GetString(decrypted);
        }

        /// <summary>
        /// 32位十六进制 → 16字节数组
        /// </summary>
        private static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length != 32)
                throw new ArgumentException("SM4 密钥/IV必须是32位十六进制字符串");

            byte[] bytes = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}
