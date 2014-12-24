using Moovers.Webservices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Moovers.WebServices.Services.Concrete
{
    public class HmacSignatureCalculator : ISignatureCalculator
    {
        public string CalculateSignature(string secret, string value)
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var valueBytes = Encoding.UTF8.GetBytes(value);
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(valueBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}