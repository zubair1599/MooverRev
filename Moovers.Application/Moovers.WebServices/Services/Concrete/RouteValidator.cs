using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Moovers.WebServices.Services.Concrete
{
    public static class RouteValidator
    {
        public static bool ValidateLookupCall(HttpRequestMessage requestMessage)
        {
            return requestMessage.RequestUri.AbsolutePath.Contains("/v1/user/lookup") ||
                   requestMessage.RequestUri.AbsolutePath.Contains("/v1/smart/quote")|| 
                   requestMessage.RequestUri.AbsolutePath.Contains("/v1/smart/images") ||
                   requestMessage.RequestUri.AbsolutePath.Contains("/v1/smart/info") ||
                   requestMessage.RequestUri.AbsolutePath.Contains("/v1/smart/file")||
                   requestMessage.RequestUri.AbsolutePath.Contains("v1/quote/load/images/");


        }
        public static bool ValidateLoginCall(HttpRequestMessage requestMessage)
        {
            return requestMessage.RequestUri.AbsolutePath.Contains("/v1/user/login");
        }
    }
}