using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Mvc;
using Business.Utility;
using Moovers.WebServices.SampleApplication.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Moovers.WebServices.SampleApplication.Controllers
{
    public class SmartController : Controller
    {
        public ActionResult Index()
        {
            var request = new RestRequest("v1/smart/info", RestSharp.Method.POST);
            var client = new RestClient(Configuration.ApiBaseUrl);

            var data = new
            {
                name = "new zubi",
                from = new
                {
                    street = "3516 main blv",
                    city = "Lahore",
                    state = "MO",
                    zip = "42222"

                },
                from_latlng = new
                {
                    latitude = "37.222222222",
                    longitude = "-9.22222222"
                },
              
                to = new
                {
                    street = "4222 cantt",
                    city = "Lahore",
                    state = "MO",
                    zip = "42000"

                },
                to_latlng = new
                {
                    latitude = "37.222222222",
                    longitude = "-9.22222222"
                },
                move_date = "2012-12-07T22:34:29.8357508Z", // utc,
                email = "test@gmail.com",
                telephone = "2222227881"
            };

            request.AddParameter("text/json", data.SerializeToJson(), ParameterType.GetOrPost);


            IRestResponse res2 = client.Execute(request);
            object res =
                JsonSerializer.Create()
                    .Deserialize(new JsonTextReader(new StringReader(res2.Content.Replace("text/json=", ""))));
            var jObject = res as JObject;

            Helper.MoveLookup = jObject["lookup"].ToString();
            return new ContentResult() {Content = res2.Content};

        }

        public ActionResult Upload()
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    // Make sure to change API address 
                    client.BaseAddress = new Uri(Configuration.ApiBaseUrl);
                    var path = Path.GetFullPath("c:\\1.png");
                  var fileExist = System.IO.File.Exists( path);
                    // Add first file content 
                    var fileContent1 =
                        new ByteArrayContent(System.IO.File.ReadAllBytes(path));
                    fileContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("FileAttachment")
                    {
                        FileName = "test.png",
                        Name = "4223"
                    };
                    //  Add Second file content 
                    var fileContent2 =
                        new ByteArrayContent(System.IO.File.ReadAllBytes(@"c:\\2.jpeg"));
                    fileContent2.Headers.ContentDisposition = new ContentDispositionHeaderValue("FileAttachment")
                    {
                        FileName = "Sample.png",
                        Name = "4223"
                    };
                    content.Add(fileContent1);

                    content.Add(fileContent2);
                    
                    content.Headers.Add("lookup", Helper.MoveLookup);
                    // Make a call to Web API 
                    var result = client.PostAsync("/v1/quote/load/images/", content).Result;
                    string data = Uri.UnescapeDataString(result.Content.ReadAsStringAsync().Result);
                    return new ContentResult() {Content = data};
                }
            }
        }
    }
}
