// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="HomeController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.WebServices.SampleApplication.Controllers
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Mvc;

    using Business.Models;
    using Business.Utility;

    using Moovers.WebServices.SampleApplication.Models;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using RestSharp;

    public class HomeController : Controller
    {
        private const string username = "clay";

        public ActionResult Index()
        {
            var sampleRequest = new SampleRequest() { Date = DateTime.Now.ToUniversalTime(), Username = username };
            sampleRequest.Content = string.Empty; // no content for now
            sampleRequest.Method = Method.GET; // get for user lookup

            sampleRequest.Parameters = new NameValueCollection { { "user_name", username } }; // parameter for lookup call
            IRestResponse response = sampleRequest.ExecuteRequest("v1/user/lookup", "");
            // initial path but no session token

            Parameter keyheader = response.Headers.FirstOrDefault(k => k.Name.Equals("privateKey"));
            if (keyheader != null)
            {
                Helper.PrivateKey = keyheader.Value.ToString();
            }

            sampleRequest = GetSampleUserData();

            sampleRequest.Method = Method.POST;
            sampleRequest.PrivateKey = Helper.PrivateKey;
            sampleRequest.Parameters = new NameValueCollection { { "user_name", username }, { "password", "abc123!" } };
            sampleRequest.Content = (new ExternalUser() { user_name = username, password = "abc123!" }).SerializeToJson();
            sampleRequest.Content = sampleRequest.Content.Replace(@"\""", "\"");

            response = sampleRequest.ExecuteRequest("v1/user/login", "");

            string cleanContent = Uri.UnescapeDataString(response.Content);
            object data = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(cleanContent.Replace("text/json=", ""))));
            var jObject = data as JObject;

            var inner = jObject["data"].Value<JObject>();
            var user_id = inner["user_id"].Value<JToken>();

            if (keyheader != null)
            {
                Helper.UserId = user_id.ToString();
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return Content(response.Content);
            }

            sampleRequest.Method = Method.GET;
            Parameter token = response.Headers.FirstOrDefault(k => k.Name.Equals("session_token"));
            if (token != null)
            {
                Helper.CurrentToken = token.Value.ToString();
            }
            return Content(response.Content);
        }

        public ActionResult Schedule()
        {
            SampleRequest sampleRequest = GetSampleUserData();

            DateTime dayToShow = DateTime.Today.ToUniversalTime();

            var param = new NameValueCollection();
            param["day"] = "6";
            param["month"] = "30";
            param["year"] = dayToShow.Year.ToString();
            sampleRequest.PrivateKey = Helper.PrivateKey;
            sampleRequest.Content = "";
            sampleRequest.Parameters = param;
            IRestResponse response = sampleRequest.ExecuteRequest("/v1/quote/list", Helper.CurrentToken);

            SampleRequest req1 = GetSampleUserData();
            var param1 = new NameValueCollection();
            param1["quotes"] = response.Content;
            /////
            req1.PrivateKey = Helper.PrivateKey;
            req1.Content = response.Content;
            req1.Parameters = param1;
            req1.Method = Method.POST;
            IRestResponse res1 = req1.ExecuteRequest("/v1/quote/update", Helper.CurrentToken);

            return Content(response.Content);
        }

        public ActionResult Stop()
        {
            try
            {
                SampleRequest req2 = GetSampleUserData();

                var conditionrep =
                    new
                    {
                        address_type = 0,
                        complex_gate_code = 123,
                        contact = "",
                        dock_high = 0,
                        elevator = 2,
                        floors = 11,
                        inside_stairs_type = 0,
                        outside_stairs_type = 1,
                        parking_type = 2,
                        requires_liftgate = 1,
                        stairs_inside = 1,
                        stairs_outside = 45,
                        walking = 100,
                    };

                var stop =
                    new
                    {
                        address1 = "6300 NW Hogan Dr Apt 7",
                        address2 = "",
                        city = "Kansas City",
                        condition = conditionrep,
                        postal_code = "64152-3992",
                        priority = 0,
                        quote_id = "96df5700-7a77-4fdd-88fe-38f30804472e",
                        state = "MO",
                        stop_id = "780222f9-7c06-424f-bf17-204c13dab5fe",
                    };

                req2.PrivateKey = Helper.PrivateKey;
                req2.Content = stop.SerializeToJson(1);
                req2.Method = Method.POST;
                IRestResponse res2 = req2.ExecuteRequest("/v1/quote/update-address-conditions", Helper.CurrentToken);

                return new ContentResult() { Content = res2.Content };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult CustomerSignOff()
        {
            SampleRequest req2 = GetSampleUserData();

            var cusSignOff =
                new
                {
                    quote_id = "7A7119AF-42B7-4E87-9543-9595480624FA",
                    stop_id = "A83160A5-CDD4-4FA5-B7D4-DDFC39FC6BF0",
                    valuation_type_id = "46637499-6b5b-4354-9c08-bc375ff64850",
                    insurance_value = 12.94,
                    confirmation_email = "someone@somewhere.com",
                    email_receipt = true,
                    feedback_survey = true,
                    future_offers = true,
                    latitude = 94.335534557,
                    longitude = -135.732894673,
                    signature =
                        new
                        {
                            signature =
                                "<?xml version='1.0'?><svg height='225.280000' width ='600.000000'>" + "<path d='M321.50 111.70"
                                + "Q321.50 111.70 321.50 111.70" + "Q321.50 111.70 320.25 106.95" + "Q319.00 102.20 321.00 94.20" + "Q323.00 86.20 323.00 86.20"
                                + "Q323.00 86.20 330.00 76.70" + "Q337.00 67.20 337.00 67.20" + "Q337.00 67.20 357.00 63.45" + "Q377.00 59.70 400.50 60.70"
                                + "Q424.00 61.70 441.75 64.95" + "Q459.50 68.20 462.50 71.95" + "Q465.50 75.70 463.00 81.45" + "Q460.50 87.20 450.00 90.45"
                                + "Q439.50 93.70 433.75 93.70" + "Q428.00 93.70 427.50 91.70" + "Q427.00 89.70 428.75 82.20" + "Q430.50 74.70 442.50 69.45"
                                + "Q454.50 64.20 474.00 57.45" + "Q493.50 50.70 506.50 46.95' stroke='black' stroke-width='2' fill='none'/>" + "</svg>",
                            signature_time = "2012-12-07T22:34Z", //UTC
                            signature_type = 1
                        }
                };

            req2.PrivateKey = Helper.PrivateKey;
            req2.Content = cusSignOff.SerializeToJson();

            req2.Method = Method.POST;
            IRestResponse res2 = req2.ExecuteRequest("/v1/quote/customer-signoff", Helper.CurrentToken);

            return new ContentResult() { Content = res2.Content };
        }

        public ActionResult InventoryVerification()
        {
            SampleRequest req2 = GetSampleUserData();

            var verificationSignOff =
                new
                {
                    quote_id = "7A7119AF-42B7-4E87-9543-9595480624FA",
                    stop_id = "A83160A5-CDD4-4FA5-B7D4-DDFC39FC6BF0",
                    email_receipts = "schow@engagemobile.com;someone@somewhere.com;whatever@nowhere.com",
                    latitude = 94.335534557,
                    longitude = -135.732894673,
                    signature = new
                    {
                        pad_width = 80,
                        pad_height = 40,
                        signature = "12,3:12,4:12:5", //x,y:x,y:x,y
                        signature_time = "2012-12-07T22:34:29.8357508Z", //UTC
                        signature_type = "2" // Enum CustomerSignOff, InventoryVerifySignOff …
                    }
                };

            req2.PrivateKey = Helper.PrivateKey;
            req2.Content = verificationSignOff.SerializeToJson();

            req2.Method = Method.POST;
            IRestResponse res2 = req2.ExecuteRequest("/v1/quote/inventory-verify-signoff", Helper.CurrentToken);

            return new ContentResult() { Content = res2.Content };
        }

        public ActionResult CheckList()
        {
            SampleRequest req2 = GetSampleUserData();

            var verificationSignOff =
                new { quote_id = "4d127cbf-08a3-4046-8b8d-3b289a98339d", q1 = 0, q2 = 1, q3 = 1, q4 = 0, q5 = 0, q6 = 1, Percentage = 30.0 };

            req2.PrivateKey = Helper.PrivateKey;
            req2.Content = verificationSignOff.SerializeToJson();

            req2.Method = Method.POST;
            IRestResponse res2 = req2.ExecuteRequest("/v1/quote/update_checklist", Helper.CurrentToken);

            return new ContentResult() { Content = res2.Content };
        }

        public ActionResult UnloadVerification()
        {
            SampleRequest req2 = GetSampleUserData();

            var verificationSignOff =
                new
                {
                    quote_id = "7A7119AF-42B7-4E87-9543-9595480624FA",
                    stop_id = "A83160A5-CDD4-4FA5-B7D4-DDFC39FC6BF0",
                    survey_id = 1, //Enum SurveyType
                    latitude = 94.335534557,
                    longitude = -135.732894673,
                    gratuity = 10.23,
                    signature = new
                    {
                        pad_width = 80,
                        pad_height = 40,
                        signature = "12,3:12,4:12:5", //x,y:x,y:x,y
                        signature_time = "2012-12-07T22:34:29.8357508Z", //UTC
                        signature_type = "2" // Enum CustomerSignOff, InventoryVerifySignOff …
                    }
                };

            req2.PrivateKey = Helper.PrivateKey;
            req2.Content = verificationSignOff.SerializeToJson();

            req2.Method = Method.POST;
            IRestResponse res2 = req2.ExecuteRequest("/v1/quote/unload-verify-signoff", Helper.CurrentToken);

            return new ContentResult() { Content = res2.Content };
        }

        public ActionResult QuoteStatus()
        {
            SampleRequest req2 = GetSampleUserData();

            var status = new
            {
                stop_id = "903f8353-27c6-4f36-8b8f-1b5bcb3c63f4",
                status_datetime = DateTime.Now, //UTC
                status_type = "8",
                latitude = 94.335534557,
                longitude = -135.732894673
            };

            req2.PrivateKey = Helper.PrivateKey;
            req2.Content = status.SerializeToJson();

            req2.Method = Method.POST;
            IRestResponse res2 = req2.ExecuteRequest("v1/quote/update-status/", Helper.CurrentToken);

            return new ContentResult() { Content = res2.Content };
        }

        public ActionResult Inventories()
        {
            var testrooms1 = new[]
            {
                new
                {
                    inventories =
                        new object[]
                        {
                            new
                            {
                                addendums =
                                    new object[]
                                    {
                                        new 
                                                {
                            addendum_id = "6fd53e53-236e-4854-ad66-07c8bc2e0156",
                            addendum_name = "Frame",
                            room_adddum_id = "7bc739dc-c395-429b-855f-357bd5befa45",
                            room_item_id = "9092ea3b-1d5a-460c-b1b1-8ea40fc761d7",
                            sub_addendum_id = "a9a30780-4d0e-4bc8-9b4d-ebbec9e6d64e"
                        }
                    
                                    },
                                count = 1,
                                item = new { cubic_feet = "37.5", item_id = "a6508408-3fb2-4c49-90c1-5a2a44e17d8a", movers_required = 2, name = "Bed (Queen)" },
                                item_id = "a6508408-3fb2-4c49-90c1-5a2a44e17d8a",
                                name = "Bed (Queen)",
                                notes = null as object[],
                                relationshipid = "9092ea3b-1d5a-460c-b1b1-8ea40fc761d7",
                                roomid = "43d80cc9-16bd-4684-8819-3faf5c7973ac",
                                storagecount = 0
                            },
                            new
                            {
                                addendums = null as object[],
                                count = 2,
                                item = new { cubic_feet = "25", item_id = "bf95071b-52fb-497c-b913-83085192eacc", movers_required = 2, name = "Chest of Drawers" },
                                item_id = "bf95071b-52fb-497c-b913-83085192eacc",
                                name = "Chest of Drawers",
                                notes = null as object[],
                                relationshipid = "d3ec974d-c113-466e-bfe2-7c05680385ef",
                                roomid = "43d80cc9-16bd-4684-8819-3faf5c7973ac",
                                storagecount = 0
                            },
                            new
                            {
                                addendums = null as object[],
                                count = 2,
                                item = new { cubic_feet = "0.75", item_id = "cbbd8b20-1358-4d46-8998-ed962855776f", movers_required = 1, name = "Picture (<24x24''')", },
                                item_id = "cbbd8b20-1358-4d46-8998-ed962855776f",
                                name = "Picture (<24x24''')",
                                notes = null as object[],
                                relationshipid = "1a3b3344-90a4-4d65-a396-96b0bf3fa83b",
                                roomid = "43d80cc9-16bd-4684-8819-3faf5c7973ac",
                                storagecount = 0,
                            },
                            new
                            {
                                addendums = null as object[],
                                count = 2,
                                item = new { cubic_feet = "9", item_id = "cf88e575-a672-41ec-be96-7912fbebadec", movers_required = 1, name = "Headboard", },
                                item_id = "cf88e575-a672-41ec-be96-7912fbebadec",
                                name = "Headboard",
                                notes = null as object[],
                                relationshipid = "edbbedbb-4f30-4ffe-b186-e644786f4765",
                                roomid = "43d80cc9-16bd-4684-8819-3faf5c7973ac",
                                storagecount = 0,
                            }
                        },
                    name = "Unassigned",
                    room_id = "43d80cc9-16bd-4684-8819-3faf5c7973ac",
                    description = ""
                }
            };

            var newdata =
                new
                {
                    quote_id = "a156d56e-86bc-4dcf-af40-4c280c2c09f5",
                    stop_id = "dd24277f-9224-47d7-b52c-57d06ff877c4",
                    update_type = "walk-thru",
                    rooms = testrooms1
                };

            SampleRequest req = GetSampleUserData();
            req.PrivateKey = Helper.PrivateKey;
            string datajson = newdata.SerializeToJson();
            req.Content = datajson;
            req.Method = Method.POST;
            IRestResponse res = req.ExecuteRequest("/v1/quote/inventory-update", Helper.CurrentToken);

            return new ContentResult() { Content = res.Content };
        }

        public ActionResult Enums()
        {
            SampleRequest sampleRequest = GetSampleUserData();

            DateTime dayToShow = DateTime.Today.ToUniversalTime();

            sampleRequest.PrivateKey = Helper.PrivateKey;
            sampleRequest.Content = "";

            IRestResponse response = sampleRequest.ExecuteRequest("/v1/general/code", Helper.CurrentToken);
            return Content(response.Content);
        }

        public ActionResult GetInventories()
        {
            SampleRequest sampleRequest = GetSampleUserData();

            DateTime dayToShow = DateTime.Today.ToUniversalTime();

            sampleRequest.PrivateKey = Helper.PrivateKey;
            sampleRequest.Content = "";

            IRestResponse response = sampleRequest.ExecuteRequest("/v1/general/inventories", Helper.CurrentToken);
            return Content(response.Content);
        }

        public ActionResult GetValuations()
        {
            SampleRequest sampleRequest = GetSampleUserData();

            DateTime dayToShow = DateTime.Today.ToUniversalTime();

            sampleRequest.PrivateKey = Helper.PrivateKey;
            sampleRequest.Content = "";

            IRestResponse response = sampleRequest.ExecuteRequest("/v1/general/valuations", Helper.CurrentToken);
            return Content(response.Content);
        }

        public ActionResult LogOut()
        {
            SampleRequest sampleRequest = GetSampleUserData();
            sampleRequest.Content = "";
            sampleRequest.PrivateKey = Helper.PrivateKey;
            sampleRequest.Method = Method.POST;
            var data = new { user_id = Helper.UserId };
            sampleRequest.Content = data.SerializeToJson();
            IRestResponse response = sampleRequest.ExecuteRequest("/v1/user/logout", Helper.CurrentToken);
            return Content(response.Content);
        }

        public ActionResult Payments()
        {
            var payment =
                new
                {
                    amount = 12,
                    billingZip = "",
                    cardnumber = "",
                    cvv2 = "",
                    expirationmonth = "",
                    expirationyear = "",
                    memo = "none",
                    checkNumber = "",
                    personalCheckNumber = "",
                    payment_signature =
                        new
                        {
                            signature =
                                "<?xml version='1.0'?><svg height='225.280000' width ='600.000000'>" + "<path d='M321.50 111.70"
                                + "Q321.50 111.70 321.50 111.70" + "Q321.50 111.70 320.25 106.95" + "Q319.00 102.20 321.00 94.20" + "Q323.00 86.20 323.00 86.20"
                                + "Q323.00 86.20 330.00 76.70" + "Q337.00 67.20 337.00 67.20" + "Q337.00 67.20 357.00 63.45" + "Q377.00 59.70 400.50 60.70"
                                + "Q424.00 61.70 441.75 64.95" + "Q459.50 68.20 462.50 71.95" + "Q465.50 75.70 463.00 81.45" + "Q460.50 87.20 450.00 90.45"
                                + "Q439.50 93.70 433.75 93.70" + "Q428.00 93.70 427.50 91.70" + "Q427.00 89.70 428.75 82.20" + "Q430.50 74.70 442.50 69.45"
                                + "Q454.50 64.20 474.00 57.45" + "Q493.50 50.70 506.50 46.95' stroke='black' stroke-width='2' fill='none'/>" + "</svg>",
                            signature_time = "2014-12-07T22:34Z", //UTC
                            signature_type = 6
                        }
                };

            var newdata = new { payment_info = payment };

            SampleRequest req = GetSampleUserData();
            req.PrivateKey = Helper.PrivateKey;
            string datajson = newdata.SerializeToJson();
            req.Content = datajson;
            req.Method = Method.POST;
            IRestResponse res = req.ExecuteRequest("/v1/payment/authorize", Helper.CurrentToken);

            return new ContentResult() { Content = res.Content };
        }

        public ActionResult Card()
        {
            string quote_lookup = "44086";

            var newdata = new { quote_lookup = quote_lookup };

            SampleRequest req = GetSampleUserData();
            req.PrivateKey = Helper.PrivateKey;
            string datajson = newdata.SerializeToJson();
            req.Content = datajson;
            req.Method = Method.POST;
            IRestResponse res = req.ExecuteRequest("/v1/payment/quote", Helper.CurrentToken);

            return new ContentResult() { Content = res.Content };
        }

        public ActionResult ExistingPayments()
        {
            string quote_lookup = "43349";

            var newdata = new { quote_lookup = quote_lookup };

            SampleRequest req = GetSampleUserData();
            req.PrivateKey = Helper.PrivateKey;
            string datajson = newdata.SerializeToJson();
            req.Content = datajson;
            req.Method = Method.POST;
            IRestResponse res = req.ExecuteRequest("/v1/payment/list", Helper.CurrentToken);

            return new ContentResult() { Content = res.Content };
        }

        public ActionResult Directions()
        {
            var newdata =
                new
                {
                    quote_id = "7a7119af-42b7-4e87-9543-9595480624fa",
                    latitude = -12.044012922866312,
                    longitude = -77.02470665341184,
                    checkin_time = DateTime.UtcNow
                };

            SampleRequest req = GetSampleUserData();
            req.PrivateKey = Helper.PrivateKey;
            string datajson = newdata.SerializeToJson();
            req.Content = datajson;
            req.Method = Method.POST;
            IRestResponse res = req.ExecuteRequest("v1/quote/update-location/", Helper.CurrentToken);

            return new ContentResult() { Content = res.Content };
        }

        public ActionResult Upload()
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    // Make sure to change API address 
                    client.BaseAddress = new Uri(Configuration.ApiBaseUrl);
                    string path = Path.GetFullPath("c:\\1.png");
                    bool fileExist = System.IO.File.Exists(path);
                    // Add first file content 
                    var fileContent1 = new ByteArrayContent(System.IO.File.ReadAllBytes(path));
                    fileContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("FileAttachment") { FileName = "test.png", Name = "4223" };
                    //  Add Second file content 
                    var fileContent2 = new ByteArrayContent(System.IO.File.ReadAllBytes(@"c:\\2.jpeg"));
                    fileContent2.Headers.ContentDisposition = new ContentDispositionHeaderValue("FileAttachment") { FileName = "Sample.png", Name = "4223" };
                    content.Add(fileContent1);

                    content.Add(fileContent2);

                    SampleRequest req = GetSampleUserData();
                    req.PrivateKey = Helper.PrivateKey;
                    var uri = new Uri(new Uri(Configuration.ApiBaseUrl), path);
                    string canonicalRepresentation = HmacUtility.GetCanonicalRepresentation(req.Method, req.Username, "", uri.AbsolutePath, req.Date);

                    string signature = HmacUtility.CalculateSignature(req.PrivateKey, canonicalRepresentation);

                    var authenticationHeaderValue = new AuthenticationHeaderValue(Configuration.AuthorizationMethod, signature);

                    client.DefaultRequestHeaders.Add("quote_lookup", "41233");
                    client.DefaultRequestHeaders.Add(Configuration.UsernameHeader, req.Username);
                    string dt = req.Date.ToUniversalTime().ToString();
                    client.DefaultRequestHeaders.Add("RequestDateTimeStamp", dt);
                    client.DefaultRequestHeaders.Add("Authorization", authenticationHeaderValue.ToString());
                    client.DefaultRequestHeaders.Add(HttpRequestHeader.ContentMd5.ToString(), HmacUtility.GetMd5Hash(req.Content));
                    content.Headers.Add("session_token", Helper.CurrentToken);

                    // Make a call to Web API 
                    HttpResponseMessage result = client.PostAsync("/v1/quote/load/images/", content).Result;
                    string data = Uri.UnescapeDataString(result.Content.ReadAsStringAsync().Result);
                    return new ContentResult() { Content = data };
                }
            }
        }

        private SampleRequest GetSampleUserData()
        {
            return new SampleRequest { Date = DateTime.Now.ToUniversalTime(), Username = username, };
        }
    }
}