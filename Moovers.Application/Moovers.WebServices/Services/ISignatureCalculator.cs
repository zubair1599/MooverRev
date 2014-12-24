using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Moovers.Webservices.Services
{
    public interface ISignatureCalculator
    {
        string CalculateSignature(string secret, string value);
    }
}