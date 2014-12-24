using System.Data.Objects.DataClasses;
using System.Text;
using System.Web;
using System.Web.Security;
using Business.Interfaces;
using Business.Models;
using Business.Repository.Models;
using System;
using System.Linq;
using Business.Utility;
using LinqKit;
using RestSharp.Extensions;

namespace Business.Repository
{
    public class EmployeeAuthenticationRepository : RepositoryBase<aspnet_User>,
        ICustomAuthenticationRepository
    {
        private ICacheRepository _cacheRepository;

        public EmployeeAuthenticationRepository(ICacheRepository cacheRepository)
        {
            _cacheRepository = cacheRepository;
        }

        public string GetSecretForUser(string username)
        {
            var user = this.Get(username);
            if (user != null)
            {
                var aspnetUsersProfile = user.aspnet_Users_Profile.FirstOrDefault();
                if (aspnetUsersProfile != null)
                    return aspnetUsersProfile.PrivateKey.ToString();
            }

            return null;
        }

        public aspnet_User Authenticate(string username, string password)
        {
            var user = this.Get(username);
            if (user == null)
            {
                return null;
            }

            if (!Membership.ValidateUser(username,password))
            {
                return null;
            }

           user.CurrentSession = this.CreateNewSession(user);

            return user;
        }


        public aspnet_User GetMooverForUser(string username)
        {
            return this.Get(username);
        }

        public override aspnet_User Get(Guid id)
        {
            return db.aspnet_Users.SingleOrDefault(i => i.UserId == id);
        }

        public aspnet_User Get(string username)
        {
            return db.aspnet_Users.SingleOrDefault(i => i.UserName == username);
        }

        public EmployeeAuth_Session CreateNewSession(aspnet_User user)
        {
            //deactivate all previous active sections
            var usersessions = db.EmployeeAuth_Session.Where(s => s.UserID == user.UserId && s.IsActive);
            usersessions.ForEach(s =>
            {
                s.IsActive = false;
            });

            if (user.EmployeeAuth_Session == null)
                user.EmployeeAuth_Session = new EntityCollection<EmployeeAuth_Session>();
            
            user.EmployeeAuth_Session.Add(new EmployeeAuth_Session
            {             
                IsActive = true,
                IssuedOn = DateTime.Now,
                ExpiredOn = DateTime.Now.AddDays(1)
            });

            this.Save();

            return db.EmployeeAuth_Session.FirstOrDefault(s => s.IsActive);
        }

        public bool ValidateToken(string username, string token)
        {
            //Always get from session
            if (_cacheRepository.Contains("Session_" + username))
            {
                var storedToken = _cacheRepository.Get("Session_" + username);
                return token.Equals(storedToken.ToString());
            }

            return false;
        }
        public bool ExpireCurrentSession(Guid userID)
        {
            //using (var en = new MooversCRMEntities())
            //{
                var user = this.Get(userID);
                var userSession = db.EmployeeAuth_Session.Where(s => s.IsActive && s.UserID == user.UserId);
                userSession.ToList().ForEach(s =>
                {
                    s.IsActive = false;
                    s.ExpiredOn = DateTime.Now;
                });
                _cacheRepository.Clear("Session_" + user.UserName);
                this.Save();
            //    en.SaveChanges();
            //}
           
            
            return true;
        }

        public void LogRequest(string userName,StringBuilder request)
        {
            try
            {
                //using (var en = new MooversCRMEntities())
                //{
                    var user = this.Get(userName);
                    user.RequestLogs.Add(new RequestLog()
                    {
                        CreatedDate = DateTime.Now,
                        Requestlog1 = request.ToString()
                    });
                    this.Save();
                //    en.SaveChanges();
                //}
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
