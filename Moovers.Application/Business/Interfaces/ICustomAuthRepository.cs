using System;
using System.Text;
using System.Web;
using Business.Models;

namespace Business.Interfaces
{
    public interface ICustomAuthenticationRepository
    {
        string GetSecretForUser(string username);

        aspnet_User GetMooverForUser(string username);

        aspnet_User Authenticate(string username, string password);

        bool ExpireCurrentSession(Guid userID);

        EmployeeAuth_Session CreateNewSession(aspnet_User user);

        bool ValidateToken(string username, string token);

        void LogRequest(string username,StringBuilder request);
    }
}
