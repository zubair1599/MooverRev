using Business.Repository.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Business.Utility
{
    public static class AddressVerification
    {
        private static readonly string ApiUrl = System.Configuration.ConfigurationManager.AppSettings["SmartyStreetUrl"];

        private static readonly string AuthID = System.Configuration.ConfigurationManager.AppSettings["SmartyStreetID"];

        private static readonly string AuthToken = System.Configuration.ConfigurationManager.AppSettings["SmartyStreetToken"];

        public static IEnumerable<CandidateAddress> GetCandidates(Models.Address address)
        {
            // the address verification will always fail with an incomplete address -- no reason to waste an API request on a pointless lookup.
            if (String.IsNullOrEmpty(address.City) || String.IsNullOrEmpty(address.State) || String.IsNullOrEmpty(address.Street1) || String.IsNullOrEmpty(address.Zip))
            {
                return Enumerable.Empty<CandidateAddress>();
            }

            //often times, sales people enter in address like "*** Calling In ***, Kansas City, MO or something similar. We might as well not waste an API lookup on these.
            if (address.Street1.StartsWith("*"))
            {
                return Enumerable.Empty<CandidateAddress>();
            }

            var repo = new SmartyStreetsCacheRepository();
            var cache = repo.GetFromAddress(address);

            if (cache == null)
            {
                var client = new RestSharp.RestClient(ApiUrl);
                client.ClearHandlers();
                client.AddHandler("application/json", new RestSharp.Deserializers.JsonDeserializer());

                var req = new RestSharp.RestRequest(RestSharp.Method.GET);
                req.AddParameter("auth-id", AuthID);
                req.AddParameter("auth-token", AuthToken);
                req.AddParameter("street", address.Street1 + " " + address.Street2);
                req.AddParameter("city", address.City);
                req.AddParameter("state", address.State);
                req.AddParameter("zipCode", address.Zip);

                var resp = client.Execute(req);
                if (resp.StatusCode == System.Net.HttpStatusCode.OK && resp.Content != null)
                {
                    cache = new Models.SmartyStreetsCache(address, resp.Content);
                    repo.Add(cache);
                    repo.Save();
                }
                else
                {
                    cache = new Models.SmartyStreetsCache(address, String.Empty);
                    repo.Add(cache);
                    repo.Save();
                }
            }

            if (String.IsNullOrEmpty(cache.Response))
            {
                return Enumerable.Empty<CandidateAddress>();
            }

            return JsonConvert.DeserializeObject<IEnumerable<CandidateAddress>>(cache.Response);
        }
    }
}
