using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Business.Repository.Models;
using FluentValidation;
using FluentSecurity;

namespace MooversCRM
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;

            ViewEngines.Engines.Add(new RazorViewEngine());
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            var repo = new ErrorRepository();

            var error = context.Server.GetLastError();
            if (error is HttpException && ((HttpException)error).GetHttpCode() == 404)
            {
                Server.ClearError();
                context.Response.StatusCode = 404;
                var routedata = new RouteData();
                routedata.Values.Add("controller", "Public");
                routedata.Values.Add("action", "Error404");
                IController errorController = new MooversCRM.Controllers.PublicController();
                errorController.Execute(new RequestContext(new HttpContextWrapper(Context), routedata));
                return;
            }

            if (error.InnerException != null)
            {
                error = error.InnerException;
            }

            repo.Log(error, context.Request.Url.ToString(), context.Request.ServerVariables, context.Request.Form);
            repo.Save();

#if !DEBUG
            context.Server.ClearError();
            context.Response.StatusCode = 500;
            context.Response.Redirect("~/Error");
#endif
        }
    }
}