// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="LoginController.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.WebServices.Controllers
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Security;

    using Business.Interfaces;
    using Business.Models;
    using Business.Repository.Models;

    using Moovers.Webservices.Controllers;

    public class LoginController : ControllerBase
    {
        private readonly ICustomAuthenticationRepository _authenticationRepo;

        private readonly ICacheRepository _cacheRepository;

        public LoginController(ICustomAuthenticationRepository authenticationRepo, ICacheRepository cacheRepository)
        {
            _authenticationRepo = authenticationRepo;
            _cacheRepository = cacheRepository;
        }

        [HttpGet]
        [Route("v1/user/lookup")]
        public HttpResponseMessage Get(string user_name)
        {
            string key = _authenticationRepo.GetSecretForUser(user_name);
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK, new { Message = "key found", Data = "Key :" + key });
            response.Headers.Add("privateKey", key);
            return response;
        }

        [HttpPost]
        [Route("v1/user/login")]
        public HttpResponseMessage Post(ExternalUser user)
        {
            if (user == null)
            {
                return Request.CreateResponse(
                      HttpStatusCode.OK,
                      new { status = "failed", title = "", message = "user data is empty" });

            }
            aspnet_User empAuth = _authenticationRepo.Authenticate(user.user_name, user.password);
            if (empAuth == null)
            {
                FormsAuthentication.SignOut();
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid username or password"));
            }
            else

            {
                _cacheRepository.Clear("Session_" + user.user_name);

                HttpResponseMessage response = Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { status = "success", title = "", message = "key found", data = new { user_id = empAuth.UserId, login_success = true } });

                if (empAuth.CurrentSession != null)
                {
                    _cacheRepository.Store("Session_" + user.user_name, empAuth.CurrentSession.SessionToken.ToString());
                    response.Headers.Add("session_token", empAuth.CurrentSession.SessionToken.ToString());
                }
                return response;
            }
        }
    }
}