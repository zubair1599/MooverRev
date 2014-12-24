using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Moovers.WebServices.Services
{
    public interface IRouteBuilder
    {
        void Register(HttpConfiguration config,IList<DelegatingHandler> handlers);
    }
}