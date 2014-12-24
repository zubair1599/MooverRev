using System;
using System.IO;
using Business.Interfaces;
using Business.Models;
using Moovers.Webservices.Controllers;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Moovers.WebServices.Controllers
{
    public class LogoutController : ControllerBase
    {

        private readonly ICustomAuthenticationRepository _authenticationRepo;

        public LogoutController(ICustomAuthenticationRepository authenticationRepository)
        {

            _authenticationRepo = authenticationRepository;
        }

        [HttpPost]
        [Route("v1/user/logout")]
        //[Authorize]
        public HttpResponseMessage Post(HttpRequestMessage request)
        {
            string data = Uri.UnescapeDataString(request.Content.ReadAsStringAsync().Result);

            var cleanedData = JsonSerializer.Create().Deserialize(new JsonTextReader(new StringReader(data.Replace("text/json=", "")))) as JObject;
          
            var user_id = cleanedData["user_id"].ToString();

            if (user_id == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            _authenticationRepo.ExpireCurrentSession(new Guid(user_id));

            return Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    Message = "User logged out successfuly",
                    Data = new {logout_success = true}
                });
        }
    }
}
