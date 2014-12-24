using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Repository.Models;

namespace Business.Utility
{
    public static class DistanceCalculator
    {
        public const double KansasCityLatitude = 42.2663614;

        public const double KansasCityLongitude = -83.2113404;

        private static readonly string MapsUrl = System.Configuration.ConfigurationManager.AppSettings["MapDirectionurl"];

        internal static GoogleMapsResult GetMapsResult(LatLng start, LatLng end)
        {
            var cacheRepo = new CachedMapResponseRepository();
            var cached = cacheRepo.GetByCoordinates(start, end);
            if (cached == null)
            {
                var url = String.Format(MapsUrl, start.ToString(), end.ToString());
                var client = new RestSharp.RestClient(url);
                var req = new RestSharp.RestRequest(RestSharp.Method.GET);
                var result = client.Execute(req);

                var json = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleMapsResult>(result.Content);
                if (result.StatusCode == System.Net.HttpStatusCode.OK && json.status == "OK")
                {
                    cached = new Models.CachedMapResponse(json.SerializeToJson()) {
                        Coordinate1 = start.ToString(),
                        Coordinate2 = end.ToString()
                    };
                    cacheRepo.Add(cached);
                    cacheRepo.Save();
                }
                else
                {
                    throw new InvalidMapsRequestException("Invalid Google Maps Request");
                }
            }

            var maps = Newtonsoft.Json.JsonConvert.DeserializeObject<GoogleMapsResult>(cached.Response);
            if (maps.status != "OK")
            {
                throw new InvalidMapsRequestException("Invalid Google Maps Request");
            }

            if (!maps.routes.Any() || !maps.routes.First().legs.Any())
            {
                throw new MapNotFoundException("Google maps could not find a request");
            }

            return maps;
        }
    }
}
