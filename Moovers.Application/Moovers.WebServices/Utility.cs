using Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Moovers.WebServices
{
    public static class Utility
    {

        public static void LogHttpRequest(string userName,ICustomAuthenticationRepository repo, object data,string request)
        {
          var sb = new StringBuilder();
            sb.Append(data);
            sb.Append(request.ToString());
            repo.LogRequest(userName, sb);
        }
    }
}