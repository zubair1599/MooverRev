using Business.Interfaces;
using Business.Models;
using System.Linq;
using System.Web.Http;

namespace Moovers.Webservices.Controllers
{
    public abstract class ControllerBase : ApiController
    {
        public virtual ICustomAuthenticationRepository UserRepository { get; set; }

        protected string UsernameHeader
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["UsernameHeader"];
            }
        }

        protected string GetUserName()
        {
            return Request.Headers.GetValues(UsernameHeader).First();
        }

        protected aspnet_User GetCurrentUser()
        {
            return this.UserRepository.GetMooverForUser(this.GetUserName());
        }
    }
}