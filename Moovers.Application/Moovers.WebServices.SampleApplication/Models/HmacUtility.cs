using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Moovers.WebServices.SampleApplication.Models
{
    public class HmacUtility
    {
        public static string CalculateSignature(string secret, string value)
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var valueBytes = Encoding.UTF8.GetBytes(value);
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(valueBytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static string GetMd5Hash(string message)
        {

            using (var md5 = MD5.Create())
            {
                var text = message ?? "TextToHash";
                byte[] retVal = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public static string GetCanonicalRepresentation(Method method, string username, string message, string path, DateTime date)
        {
            var md5 = GetMd5Hash(message);
            return String.Join("\n", new string[] {
                method.ToString(),
                md5, 
                date.ToUniversalTime().ToString(CultureInfo.InvariantCulture),
                username,
                path
            });
        }
    }
}