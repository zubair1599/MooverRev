using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Moovers.WebServices.SampleApplication.Controllers;
using System.Globalization;
using System.Net.Http.Headers;
using System.Collections.Specialized;

namespace Moovers.WebServices.SampleApplication.Models
{
    public class SampleRequest
    {
        public DateTime Date { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string PrivateKey { get; set; }

        public NameValueCollection Parameters { get; set; }

        public string Content { get; set; }

        public Method Method { get; set; }

        public IRestResponse ExecuteRequest(string path,string token)
        {
            var uri = new Uri(new Uri(Configuration.ApiBaseUrl), path);
            var request = new RestRequest(path, this.Method);
            var client = new RestClient(Configuration.ApiBaseUrl);
            if (!path.Contains("v1/user/lookup"))
            {
                var canonicalRepresentation = HmacUtility.GetCanonicalRepresentation(this.Method, this.Username,
                    this.Content, uri.AbsolutePath, this.Date);

                var signature = HmacUtility.CalculateSignature(this.PrivateKey, canonicalRepresentation);
                
                var authenticationHeaderValue = new AuthenticationHeaderValue(Configuration.AuthorizationMethod,
                    signature);

                request.AddHeader(Configuration.UsernameHeader, this.Username);
                var dt = this.Date.ToUniversalTime().ToString();
                request.AddHeader("RequestDateTimeStamp", dt);
                request.AddHeader(HttpRequestHeader.Authorization.ToString(), authenticationHeaderValue.ToString());
                request.AddHeader(HttpRequestHeader.ContentMd5.ToString(), HmacUtility.GetMd5Hash(this.Content));
                request.AddHeader("session_token", token);
            }
            if (this.Parameters != null)
            {
                foreach (var param in this.Parameters.AllKeys)
                {
                    request.AddParameter(param, this.Parameters[param]);
                }
            }

            request.AddParameter("text/json", this.Content, ParameterType.GetOrPost);
            return client.Execute(request);
        }
        public IRestResponse ExecuteFirstRequest(string path)
        {
            var uri1 = new Uri(Configuration.ApiBaseUrl);
            var uri = new Uri(uri1, path);

          
            var client = new RestClient(Configuration.ApiBaseUrl);
            var request = new RestRequest(path, this.Method);
           


            if (this.Parameters != null)
            {
                foreach (var param in this.Parameters.AllKeys)
                {
                    request.AddParameter(param, this.Parameters[param]);
                }
            }

            request.AddParameter("text/json", this.Content, ParameterType.GetOrPost);
            return client.Execute(request);
        }
    }
}