using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Text;

namespace EncryptTool.Services
{
    public sealed record Sm2KeyPair(string PublicKey, string PrivateKey);

    public static class Sm2Helper
    {
        private static readonly X9ECParameters CurveParameters =
            GMNamedCurves.GetByName("sm2p256v1")
            ?? throw new InvalidOperationException("当前加密库不支持SM2曲线");

        private static readonly ECDomainParameters DomainParameters = new(
            CurveParameters.Curve,
            CurveParameters.G,
            CurveParameters.N,
            CurveParameters.H);

        public static Sm2KeyPair GenerateKeyPair()
        {
            var generator = new ECKeyPairGenerator();
            generator.Init(new ECKeyGenerationParameters(DomainParameters, new SecureRandom()));

            AsymmetricCipherKeyPair pair = generator.GenerateKeyPair();
            var publicKey = (ECPublicKeyParameters)pair.Public;
            var privateKey = (ECPrivateKeyParameters)pair.Private;

            return new Sm2KeyPair(
                Convert.ToHexString(publicKey.Q.GetEncoded(false)).ToLowerInvariant(),
                Convert.ToHexString(ToFixedLength(privateKey.D.ToByteArrayUnsigned(), 32)).ToLowerInvariant());
        }

        public static string Encrypt(
            string data,
            string publicKey,
            string cipherMode = "C1C3C2",
            string charset = "UTF-8",
            string outputFormat = "Base64")
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("请输入需要加密的内容");

            ECPublicKeyParameters key = ParsePublicKey(publicKey);
            Encoding encoding = GetEncoding(charset);
            byte[] plainBytes = encoding.GetBytes(data);

            var engine = new SM2Engine(ParseMode(cipherMode));
            engine.Init(true, new ParametersWithRandom(key, new SecureRandom()));
            byte[] cipherBytes = engine.ProcessBlock(plainBytes, 0, plainBytes.Length);

            return FormatCipherText(cipherBytes, outputFormat);
        }

        public static string Decrypt(
            string data,
            string privateKey,
            string cipherMode = "C1C3C2",
            string charset = "UTF-8",
            string inputFormat = "Base64")
        {
            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("请输入需要解密的内容");

            ECPrivateKeyParameters key = ParsePrivateKey(privateKey);
            byte[] cipherBytes = ParseCipherText(data, inputFormat);

            var engine = new SM2Engine(ParseMode(cipherMode));
            engine.Init(false, key);
            byte[] plainBytes;
            try
            {
                plainBytes = engine.ProcessBlock(cipherBytes, 0, cipherBytes.Length);
            }
            catch (InvalidCipherTextException ex)
            {
                throw new ArgumentException("SM2解密失败，请检查私钥、密文排列和密文内容", ex);
            }

            return GetEncoding(charset).GetString(plainBytes);
        }

        public static bool IsValidPublicKey(string? value)
        {
            if (!IsHexString(value))
                return false;

            int length = value!.Length;
            return length == 130 && value.StartsWith("04", StringComparison.OrdinalIgnoreCase)
                || length == 66 &&
                (value.StartsWith("02", StringComparison.OrdinalIgnoreCase)
                 || value.StartsWith("03", StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsValidPrivateKey(string? value)
        {
            if (!IsHexString(value) || value!.Length != 64)
                return false;

            var number = new BigInteger(1, Convert.FromHexString(value));
            return number.SignValue > 0 && number.CompareTo(CurveParameters.N) < 0;
        }

        private static ECPublicKeyParameters ParsePublicKey(string publicKey)
        {
            if (!IsValidPublicKey(publicKey))
                throw new ArgumentException("SM2公钥必须是130位未压缩HEX或66位压缩HEX字符串");

            try
            {
                var point = CurveParameters.Curve.DecodePoint(Convert.FromHexString(publicKey)).Normalize();
                if (point.IsInfinity)
                    throw new ArgumentException("SM2公钥不能是无穷远点");

                return new ECPublicKeyParameters(point, DomainParameters);
            }
            catch (Exception ex) when (ex is ArgumentException or FormatException)
            {
                throw new ArgumentException("SM2公钥格式无效或不在SM2曲线上", ex);
            }
        }

        private static ECPrivateKeyParameters ParsePrivateKey(string privateKey)
        {
            if (!IsValidPrivateKey(privateKey))
                throw new ArgumentException("SM2私钥必须是有效的64位HEX字符串");

            return new ECPrivateKeyParameters(
                new BigInteger(1, Convert.FromHexString(privateKey)),
                DomainParameters);
        }

        private static SM2Engine.Mode ParseMode(string cipherMode)
        {
            return cipherMode == "C1C2C3"
                ? SM2Engine.Mode.C1C2C3
                : SM2Engine.Mode.C1C3C2;
        }

        private static string FormatCipherText(byte[] bytes, string outputFormat)
        {
            return outputFormat == "HEX"
                ? Convert.ToHexString(bytes).ToLowerInvariant()
                : Convert.ToBase64String(bytes);
        }

        private static byte[] ParseCipherText(string data, string inputFormat)
        {
            try
            {
                byte[] bytes = inputFormat == "HEX"
                    ? Convert.FromHexString(data)
                    : Convert.FromBase64String(data);

                return NormalizeCipherPointEncoding(bytes);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException($"密文不是有效的{inputFormat}格式", ex);
            }
        }

        private static byte[] NormalizeCipherPointEncoding(byte[] cipherBytes)
        {
            const int rawC1Length = 64;
            const int c3Length = 32;

            if (cipherBytes.Length == 0)
                throw new ArgumentException("SM2密文不能为空");

            if (cipherBytes[0] == 0x04)
                return cipherBytes;

            if (cipherBytes[0] == 0x30)
                throw new ArgumentException("当前密文是ASN.1格式，请先转换为C1C3C2或C1C2C3原始格式");

            if (cipherBytes.Length <= rawC1Length + c3Length)
                throw new ArgumentException("SM2密文长度不足");

            var normalized = new byte[cipherBytes.Length + 1];
            normalized[0] = 0x04;
            cipherBytes.CopyTo(normalized, 1);
            return normalized;
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

        private static bool IsHexString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length % 2 != 0)
                return false;

            foreach (char c in value)
            {
                if (!Uri.IsHexDigit(c))
                    return false;
            }

            return true;
        }

        private static byte[] ToFixedLength(byte[] value, int length)
        {
            if (value.Length > length)
                throw new ArgumentException("数值长度超过预期");

            var result = new byte[length];
            value.CopyTo(result, length - value.Length);
            return result;
        }
    }
}
