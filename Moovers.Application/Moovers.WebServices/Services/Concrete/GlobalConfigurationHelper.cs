using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Moovers.WebServices.Services.Concrete
{
    public class GlobalConfigurationHelper : IConfigurationHelper
    {
        public HttpConfiguration Configuration
        {
            get { return GlobalConfiguration.Configuration; }
        }
    }
}