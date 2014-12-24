using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Business
{
    public static class Roles
    {
        public enum CorporateRoles
        {
            Administrator,
            CallCenterSupervisor,
            CallCenterAgent,
            Claims,
            HumanResources
        }

        public enum FranchiseRoles
        {
            Manager,
            Sales,
            Dispatch,
            Moover
        }

        public static bool IsInRole(string username, CorporateRoles role)
        {
            return System.Web.Security.Roles.IsUserInRole(username, role.ToString());
        }

        public static bool IsInRole(string username, FranchiseRoles role)
        {
            return System.Web.Security.Roles.IsUserInRole(username, role.ToString());
        }

        public static bool IsInRole(string username, IEnumerable<object> roles)
        {
            return roles.Any(r => System.Web.Security.Roles.IsUserInRole(username, r.ToString()));
        }

        public static bool IsCorporateUser(IEnumerable<string> roles)
        {
            var corporate = Enum.GetNames(typeof(CorporateRoles));
            return roles.Any(r => corporate.Contains(r));
        }
    }
}
