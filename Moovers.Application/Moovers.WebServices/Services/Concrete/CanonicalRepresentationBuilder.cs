using Moovers.Webservices.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Moovers.WebServices.Services.Concrete
{
    public class CanonicalRepresentationBuilder : IMessageRepresentationBuilder
    {
        public string UsernameHeader
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["UsernameHeader"];
            }
        }
        public string DateHeader
        {
            get
            {
                return "RequestDateTimeStamp";
            }
        }
        public string SessionToken
        {
            get
            {
                return "session_token";
            }
        }

        public string AuthenticationScheme
        {
            get
            {
                return "ApiAuth";
            }
        }

        /// <summary>
        /// Builds message representation as follows:
        /// HTTP METHOD\n +
        /// Content-MD5\n +  
        /// Timestamp\n +
        /// Username\n +
        /// Request URI
        /// </summary>
        /// <returns></returns>
        public string BuildRequestRepresentation(HttpRequestMessage requestMessage)
        {
            bool valid = IsRequestValid(requestMessage);
            if (!valid)
            {
                return null;
            }

            var date = Convert.ToDateTime(requestMessage.Headers.GetValues(DateHeader).First());


            string md5 = this.GetMd5(requestMessage);
                string httpMethod = requestMessage.Method.Method;
                string username = requestMessage.Headers.GetValues(UsernameHeader).First();
                string uri = requestMessage.RequestUri.AbsolutePath.ToLower();

                var representation = new [] {
                    httpMethod,
                    md5,
                    date.ToString(CultureInfo.InvariantCulture),
                    username,
                    uri
                };

                return String.Join("\n", representation);
           
        }

        private string GetMd5(HttpRequestMessage requestMessage)
        {

            var md5 = requestMessage.Headers.GetValues(HttpRequestHeader.ContentMd5.ToString()).FirstOrDefault();
            if (requestMessage.Content == null || md5 == null)
            {
                return "";
            }

            return md5;
        }

        public bool IsRequestValid(HttpRequestMessage requestMessage)
        {
            //if (!requestMessage.Headers.Date.HasValue)
            //{
            //    return false;
            //}

            if (!requestMessage.Headers.Contains(UsernameHeader))
            {
                return false;
            }


            if (requestMessage.Headers.Authorization == null || requestMessage.Headers.Authorization.Scheme != this.AuthenticationScheme)
            {
                return false;
            }

            return true;
        }
    }
}