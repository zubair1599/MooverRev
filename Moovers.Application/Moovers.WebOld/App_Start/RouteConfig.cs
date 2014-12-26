using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MooversCRM
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               name: "Templateview",
               url: "Template",
               defaults: new { Controller = "Template", Action = "Index" }
           );

            routes.MapRoute(
                name: "emailview",
                url: "email/{id}",
                defaults: new { Controller = "Public", Action = "EmailView" }
            );

            routes.MapRoute(
                name: "unsubscribe",
                url: "unsubscribe/{email}",
                defaults: new { Controller = "Public", Action = "Unsubscribe" }
            );

            routes.MapRoute(
                name: "keepalive",
                url: "keepalive",
                defaults: new { Controller = "Home", Action = "KeepAlive" }
            );

            routes.MapRoute(
                name: "proposal",
                url: "proposal/{id}",
                defaults: new { Controller = "Public", Action = "ProposalView" }
            );

            routes.MapRoute(
                name: "Error",
                url: "Error",
                defaults: new { Controller = "Public", Action = "Error" }
            );

            routes.MapRoute(
                name: "Browser",
                url: "Browser",
                defaults: new { Controller = "Public", Action = "Browser" }
            );

            routes.MapRoute(
                name: "404",
                url: "404",
                defaults: new { Controller = "Public", Action = "404" }
            );

            routes.MapRoute(
                name: "confirm",
                url: "confirm/{id}",
                defaults: new { Controller = "Public", Action = "ConfirmMove", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "InventoryList",
                url: "inventorylist",
                defaults: new { Controller = "Home", Action = "InventoryList" }
            );

            routes.MapRoute(
                name: "PersonCreate",
                url: "Accounts/Person/Add",
                defaults: new { Controller = "Accounts", Action = "CreatePerson" }
            );

            routes.MapRoute(
                name: "PersonEdit",
                url: "Accounts/Person/Edit/{id}",
                defaults: new { Controller = "Accounts", Action = "CreatePerson" }
            );

            routes.MapRoute(
                name: "BusinessCreate",
                url: "Accounts/Business/Add",
                defaults: new { Controller = "Accounts", Action = "CreateBusiness" }
            );

            routes.MapRoute(
                name: "Stats",
                url: "stats",
                defaults: new { Controller = "Stats", action = "Index" }
            );

            routes.MapRoute(
                name: "Stats2",
                url: "stats2",
                defaults: new { Controller = "Stats", action = "Index2" }
            );
            routes.MapRoute(
                name: "NONSECURE",
                url: "home/AddExternalQuote",
                defaults: new { Controller = "NonSecure", action = "AddExternalQuote" }
            );

            routes.MapRoute(
                name: "ZipJS", 
                url: "zips",
                defaults: new { Controller = "NonSecure", action = "GetZipCodes" }
            );

            routes.MapRoute(
                name: "Search",
                url: "search",
                defaults: new { Controller = "Search", action = "Index" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}