using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Business.Utility
{
    /// <summary>
    /// </summary>
    public static class Security
    {
        private const string _key = "rEj4_Pes7E_hA4up";

        public static string HashString(string value, Guid salt)
        {
            var provider = new SHA1CryptoServiceProvider();
            var toHash = Encoding.UTF8.GetBytes(value + salt.ToString());
            var hash = provider.ComputeHash(toHash);
            return Encoding.UTF8.GetString(hash);
        }

        /// <summary>
        /// encrypt a string and return it in base64
        /// </summary>
        public static string Encrypt(string input)
        {
            var key = _key;
            var inputArray = Encoding.UTF8.GetBytes(input);
            var tripleDES = new TripleDESCryptoServiceProvider {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            var encrypter = tripleDES.CreateEncryptor();
            var resultArray = encrypter.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// <summary>
        /// Decrypt a string in base64
        /// </summary>
        public static string Decrypt(string input)
        {
            var key = _key;
            var inputArray = Convert.FromBase64String(input);
            var tripleDES = new TripleDESCryptoServiceProvider {
                Key = Encoding.UTF8.GetBytes(key),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };
            var decrypter = tripleDES.CreateDecryptor();
            var resultArray = decrypter.TransformFinalBlock(inputArray, 0, inputArray.Length);
            tripleDES.Clear();
            return Encoding.UTF8.GetString(resultArray);
        }
    }
}
