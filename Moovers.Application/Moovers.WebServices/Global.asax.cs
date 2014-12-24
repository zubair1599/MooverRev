using System.Net.Http;
using Moovers.WebServices;
using Moovers.WebServices.Services;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Mvc;
using Moovers.WebServices.Services.Concrete;

namespace Moovers.Webservices
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            var resolver = AutofacConfig.RegisterDependencies();
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            var routeBuilder = (IRouteBuilder)resolver.GetService(typeof(IRouteBuilder));
            var configuration = (IConfigurationHelper)resolver.GetService(typeof(IConfigurationHelper));
            IList<DelegatingHandler> handlers = new List<DelegatingHandler>();
            handlers.Add(resolver.GetService(typeof(HmacAuthenticationHandler)) as HmacAuthenticationHandler);            
            handlers.Add(resolver.GetService(typeof(ResponseContentMd5Handler)) as ResponseContentMd5Handler);
            handlers.Add(resolver.GetService(typeof(TokenHandler)) as TokenHandler);         
          
            routeBuilder.Register(configuration.Configuration ,handlers);
        }
    }
}