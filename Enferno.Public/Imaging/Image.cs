using System;
using System.Text;
using System.Security.Cryptography;
using System.Xml;

namespace Enferno.Public.Imaging
{
    public static class Image
    {
        /// <summary>
        /// Build a cmd param to pass to Storm image server
        /// </summary>
        /// <param name="appId">AppId for the crypto key you will be using</param>
        /// <param name="directives">Directives to pass in name=value&amp;name2=value format</param>
        /// <param name="cryptoKey">The Storm Crypto key to retreived from StormApi</param>
        /// <returns></returns>
        public static string CreateCmdParam(int appId, string directives, string cryptoKey)
        {
            var timestamp = XmlConvert.ToString(DateTime.UtcNow, "yyyyMMddHHmmss");
            var hash = SignDataWithKey(ToBase64U(Encoding.UTF8.GetBytes(string.Format("{0}{1}", timestamp, directives))), cryptoKey);
            var encodedDirectives = ToBase64U(Encoding.UTF8.GetBytes(directives));
            var url = string.Format("{0}{1}{2}{3}{4}", appId.ToString("x").PadLeft(5, '0'), timestamp, hash.Length.ToString("x").PadLeft(2, '0'), hash, encodedDirectives);
            return url;
        }
        /// <summary>
        /// Create a random key to use in crypto
        /// </summary>
        /// <param name="len">Length of the key, 48 is good for decryption and 64 for validation</param>
        /// <returns></returns>
        public static string CreateKey(int len)
        {
            // For decryptionKey pass 48, validationKey 64
            var buff = new byte[len / 2];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buff);
            var sb = new StringBuilder(len);
            foreach (byte t in buff) sb.Append(string.Format("{0:X2}", t));
            return sb.ToString();
        }

        public static string SignDataWithKey(string data, string key)
        {

            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            //32-byte hash is a bit overkill. Truncation doesn't weaking the integrity of the algorithm.
            var shorterHash = new byte[8];
            Array.Copy(hash, shorterHash, 8);
            return ToBase64U(shorterHash);
        }
        public static string ToBase64U(byte[] data)
        {
            return Convert.ToBase64String(data).Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
        }

        public static string ToBase64U(string data)
        {
            return ToBase64U(Encoding.UTF8.GetBytes(data));
        }
    }
}
