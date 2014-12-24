using System.Web;
using System.Web.Http.Batch;
using System.Web.Http.Cors;
using Business.Repository;
using Business.Repository.Models;
using Moovers.WebServices.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Moovers.WebServices.Services.Concrete;

namespace Moovers.Webservices
{
    public class RouteBuilder : IRouteBuilder
    {
        private HttpMessageHandler _handler;

        public RouteBuilder(HttpMessageHandler handler)
        {
            this._handler = handler;
        }

        public void Register(HttpConfiguration config, IList<DelegatingHandler> list)
        {
           // config.EnableCors(new EnableCorsAttribute("*", "*", "*")); 
           
            //var routeHandlers = HttpClientFactory.CreatePipeline(new HttpControllerDispatcher(config), list);
            config.MessageHandlers.Add(list[0]);
            config.MessageHandlers.Add(list[1]);
            config.MessageHandlers.Add(list[2]);
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();
            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{action}",
            //    defaults: null,
            //    constraints: null,
            //    handler: routeHandlers
            // );

            ////login route
            //config.Routes.MapHttpRoute(
            //    name: "Lookup",
            //    routeTemplate: "v1/user/lookup",
            //    defaults: new { Controller = "Login" }
            //);
          
        }
    }
}
