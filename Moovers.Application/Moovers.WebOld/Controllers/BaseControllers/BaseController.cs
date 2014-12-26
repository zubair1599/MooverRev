using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Reflection;
using Business.Repository;
using Business.Repository.Models;
using Moovers.WebModels;
using MooversCRM.Attributes;

namespace MooversCRM.Controllers.BaseControllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Override "Json" to use Newtonsoft instead of Microsoft Json serialiation
        /// </summary>
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {            
            return new Business.Utility.JsonNetResult() {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }

        protected bool IsDevelopment
        {
            get
            {
                var configval = bool.Parse(System.Configuration.ConfigurationManager.AppSettings["IsDevelopment"]);
                if (!configval)
                {
                    return false;
                }
                
                return (Request.QueryString["release"] ?? String.Empty).ToLower() != "true";
            }
        }

        protected bool IsBeta
        {
            get
            {
                return bool.Parse(System.Configuration.ConfigurationManager.AppSettings["IsBeta"]);
            }
        }

        protected bool IsDevelopmentDB
        {
            get
            {
                var conn = System.Configuration.ConfigurationManager.ConnectionStrings["MooversConnectionString"];
                return conn.ConnectionString.ToLower().Contains("sqlexpress3") || conn.ConnectionString.ToLower().Contains("beta") || conn.ConnectionString.ToLower().Contains("sqlexpress");
            }
        }

        protected string AbsoluteUrl
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["AbsoluteUrl"];
            }
        }

        public string RenderViewToString(string view, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var result = ViewEngines.Engines.FindPartialView(ControllerContext, view);
                var viewContext = new ViewContext(ControllerContext, result.View, ViewData, TempData, sw);
                result.View.Render(viewContext, sw);
                result.ViewEngine.ReleaseView(ControllerContext, result.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public IEnumerable<string> GetJavascriptFiles()
        {
            return GetResources("JavaScriptFiles");
        }

        public IEnumerable<string> GetScreenCssFiles()
        {
            return GetResources("CssFiles");
        }

        public IEnumerable<string> GetPrintCssFiles()
        {
            return GetResources("PrintCssFiles");
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ViewBag.IsDevelopment = this.IsDevelopment;
            ViewBag.IsBeta = this.IsBeta;
            ViewBag.IsDevelopmentDB = this.IsDevelopmentDB;
            ViewBag.AbsoluteUrl = this.AbsoluteUrl;

            // get the current controller name
            var currentController = filterContext.RouteData.Values["controller"];

            var currentMenu = currentController;

            // if current controller has an attribute indicating the name doesn't match it's description, use that
            var menuAttribute = filterContext.Controller.GetType().GetCustomAttributes(true).Select(i => i as MenuDescriptionAttribute).Where(i => i != null);
            if (menuAttribute.Any())
            {
                currentMenu = menuAttribute.First().MenuName;
            }

            ViewBag.SelectedMenu = currentMenu;
            ViewBag.storageCount= new HeaderModel().GetStorageAlerts(); 
            if (this.IsDevelopment)
            {
                ViewBag.JavascriptFiles = GetJavascriptFiles();
                ViewBag.ScreenCssFiles = GetScreenCssFiles();
                ViewBag.PrintCssFiles = GetPrintCssFiles();
            }

            base.OnActionExecuted(filterContext);
        }

        private IEnumerable<string> GetResources(string name)
        {
            // in development, external resources are included in the master page separately
            // on live, external resources are compiled and added together
            // this function reads the ".targets" file and gets a list of external resources
            var resourcefile = "~/ResourceList.targets";
            if (String.IsNullOrEmpty((string)HttpContext.Cache["resourcelist"]))
            {
                HttpContext.Cache["resourcelist"] = System.IO.File.ReadAllText(Server.MapPath(resourcefile));
            }

            var xmlfile = XDocument.Parse((string)HttpContext.Cache["resourcelist"]);
            var ns = xmlfile.Root.Name.Namespace.NamespaceName;
            var grouping = xmlfile.Element(XName.Get("Project", ns)).Element(XName.Get("ItemGroup", ns)).Elements(XName.Get(name, ns));
            return grouping.Select(i => i.Attribute("Include").Value.Replace("$(ProjectDir)", "~/"));
        }

        /// <summary>
        /// Generate a quote file, add it to the file repository, and return it.
        /// </summary>
        /// <param name="quoteid"></param>
        /// <param name="fileid">"-New Proposal-" to generate a new proposal and return it, "-New Contract-" to generate a new contract and return it, otherwise the ID of the file</param>
        /// <returns></returns>
        public Business.Models.File GetQuoteFile(Guid quoteid, string fileid)
        {
            var repo = new QuoteRepository();
            var quote = repo.Get(quoteid);

            if (quote == null)
            {
                throw new HttpException(404, "Not found");
            }

            ////if (!quote.CanUserRead(AspUserID))
            ////{
            ////    return HttpNotFound();
            ////}

            if (fileid == "-New Proposal-")
            {
                var view = RenderViewToString("PDFS/_Proposal", quote);
                var pdf = Business.Utility.General.GeneratePdf(view, System.Drawing.Printing.PaperKind.Letter);
                var file = new Business.Models.File("Proposal - " + DateTime.Now.ToShortDateString() + " - Quote " + quote.Lookup, "application/pdf");
                quote.AddFile(file);
                repo.Save();

                file.SavedContent = pdf;
                repo.Save();

                return file;
            }

            if (fileid == "-New Contract-")
            {
                var view = RenderViewToString("PDFS/_Contract", new ContractPrintModel(quote, quote.GetSchedules().OrderBy(i => i.Date).First(), true));
                var pdf = Business.Utility.General.GeneratePdf(view, System.Drawing.Printing.PaperKind.Legal);
                
                var file = new Business.Models.File("Contract - " + DateTime.Now.ToShortDateString() + " - Quote " + quote.Lookup, "application/pdf");
                quote.AddFile(file);
                repo.Save();

                file.SavedContent = pdf;
                repo.Save();

                return file;
            }

            if (fileid == "-New SpecialInvoice-")
            {
                var view = RenderViewToString("PDFS/_SpecialInvoice", quote);
                var pdf = Business.Utility.General.GeneratePdf(view, System.Drawing.Printing.PaperKind.Letter);
                var file = new Business.Models.File("Invoice - " + DateTime.Now.ToShortDateString() + " - Quote " + quote.Lookup, "application/pdf");
                quote.AddFile(file);
                repo.Save();
                file.SavedContent = pdf;
                repo.Save();
                return file;
            }

            if (fileid == "-New Receipt-")
            {
                ViewBag.Franchise = quote.Franchise;
                var view = RenderViewToString("PDFS/_PaymentReceipt", quote);
                var pdf = Business.Utility.General.GeneratePdf(view, System.Drawing.Printing.PaperKind.Letter);
                var file = new Business.Models.File("Receipt - " + DateTime.Now.ToShortDateString() + " - Quote " + quote.Lookup, "application/pdf");
                quote.AddFile(file);
                repo.Save();

                file.SavedContent = pdf;
                repo.Save();

                return file;
            }

            if (fileid == "-Storage Access-")
            {
                var view = RenderViewToString("PDFS/_StorageAccess", quote);
                var pdf = Business.Utility.General.GeneratePdf(view, System.Drawing.Printing.PaperKind.Letter);
                var file = new Business.Models.File("Storage Access - " + DateTime.Now.ToShortDateString() + " - Quote " + quote.Lookup, "application/pdf");
                quote.AddFile(file);
                
                repo.Save();
                file.SavedContent = pdf;
                repo.Save();

                return file;
            }

            var fileRepo = new FileRepository();
            var savedfile = fileRepo.Get(Guid.Parse(fileid));
            if (savedfile.SavedContent != null)
            {
                return savedfile;
            }

            fileRepo.Save();
            savedfile.SavedContent = Business.Utility.General.GeneratePdf(savedfile.HtmlContent, System.Drawing.Printing.PaperKind.Legal);
            savedfile.HtmlContent = null;
            fileRepo.Save();
            return savedfile;
        }
    }
}