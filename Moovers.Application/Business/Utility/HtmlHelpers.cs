using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;

namespace Business.Utility
{
    public static class HtmlHelpers
    {
        private static HttpContextBase Context
        {
            get
            {
                return new HttpContextWrapper(System.Web.HttpContext.Current);
            }
        }

        private static string VersionResource(string resource)
        {
            var versionstr = Assembly.GetExecutingAssembly().GetName().Version;
            return resource + "?v=" + versionstr;
        }

        private static RouteValueDictionary Merge(this RouteValueDictionary original, IEnumerable<KeyValuePair<string, object>> toMerge)
        {
            // Create a new dictionary containing implicit and auto-generated values
            var merged = new RouteValueDictionary(original);
            foreach (var f in toMerge)
            {
                if (merged.ContainsKey(f.Key))
                {
                    merged[f.Key] = f.Value;
                }
                else
                {
                    merged.Add(f.Key, f.Value);
                }
            }

            return merged;
        }

        public static HtmlString Javascript(this HtmlHelper helper, string src, bool version = true)
        {
            var path = UrlHelper.GenerateContentUrl(src, Context);
            if (version)
            {
                path = VersionResource(path);
            }

            return MvcHtmlString.Create(String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", path));
        }

        public static HtmlString Stylesheet(this HtmlHelper helper, string src, string media, bool version = true)
        {
            var path = UrlHelper.GenerateContentUrl(src, Context);
            if (version)
            {
                path = VersionResource(path);
            }

            return MvcHtmlString.Create(String.Format("<link rel=\"stylesheet\" href=\"{0}\" media=\"{1}\" />", path, media));
        }

        public static HtmlString ContentFor<T>(this HtmlHelper<T> helper, Expression<Func<T, object>> expr)
        {
            return helper.ContentFor(expr, null);
        }

        public static HtmlString ContentFor<T>(this HtmlHelper<T> helper, Expression<Func<T, object>> expr, object htmlattributes)
        {
            var model = helper.ViewData.Model;
            var func = expr.Compile();
            var attrs = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlattributes) as IDictionary<string, object>;
            var tb = new TagBuilder("span");
            tb.MergeAttributes(attrs, true);

            var obj = func(model);
            tb.SetInnerText(obj != null ? obj.ToString() : String.Empty);
            return new HtmlString(tb.ToString(TagRenderMode.Normal));
        }

        public static HtmlString DateBox(this HtmlHelper helper, string name)
        {
            return helper.DateBox(name, default(DateTime));
        }

        public static HtmlString DateBox(this HtmlHelper helper, string name, DateTime value)
        {
            return helper.DateBox(name, value, null);
        }

        public static HtmlString DateBox(this HtmlHelper helper, string name, DateTime value, object htmlattributes)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlattributes) as IDictionary<string, object>;
            attributes["type"] = "date";
            string strval = String.Empty;
            if (value != default(DateTime))
            {
                strval = value.ToString("yyyy-MM-dd");
            }

            return helper.TextBox(name, strval, attributes);
        }

        public static HtmlString PhoneBoxFor<T>(this HtmlHelper<T> helper, Expression<Func<T, string>> expr, object htmlAttributes)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes) as IDictionary<string, object>;
            attributes["type"] = "tel";
            attributes["pattern"] = @"\(?\d{3}\)?\-?\d{3}\-?\d{4}";
            return helper.TextBoxFor(expr, attributes);
        }

        public static HtmlString PhoneBoxFor<T>(this HtmlHelper<T> helper, Expression<Func<T, string>> expr)
        {
            return helper.PhoneBoxFor(expr, new { });
        }

        public static HtmlString DateBoxFor<T>(this HtmlHelper<T> helper, Expression<Func<T, DateTime?>> expression)
        {
            return helper.DateBoxFor(expression, new { });
        }

        public static HtmlString DateBoxFor<T>(this HtmlHelper<T> helper, Expression<Func<T, DateTime?>> expression, object htmlattributes)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlattributes) as IDictionary<string, object>;
            attributes["type"] = "date";
            return helper.TextBoxFor(expression, "{0:yyyy-MM-dd}", attributes);
        }

        public static HtmlString AjaxLoader(this HtmlHelper helper, string id, object htmlattributes = null)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlattributes) as IDictionary<string, object>;

            if (!String.IsNullOrEmpty(id))
            {
                attributes["id"] = id;
            }

            return helper.Image("~/static/img/loader.gif", "Loading...", attributes);
        }

        public static HtmlString Image(this HtmlHelper helper, string url, string alt, object htmlAttributes = null)
        {
            var obj = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            return helper.Image(url, alt, obj);
        }

        private static HtmlString Image(this HtmlHelper helper, string url, string alt, IDictionary<string, object> htmlAttributes)
        {
            // Instantiate a UrlHelper
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);

            // Create tag builder
            var builder = new TagBuilder("img");

            // Add attributes
            builder.MergeAttribute("src", urlHelper.Content(url));
            builder.MergeAttribute("alt", alt);

            builder.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            // Render tag
            return new HtmlString(builder.ToString(TagRenderMode.SelfClosing));
        }

        public static MvcHtmlString ActionLinkPreserveQuery(this HtmlHelper helper, string linktext, string action, object routeValues, object htmlAttributes = null)
        {
            var c = helper.ViewContext.RequestContext.HttpContext.Request.QueryString;
            var r = new RouteValueDictionary();
            foreach (var s in c.AllKeys)
            {
                r.Add(s, c[s]);
            }

            var htmlAtts = new RouteValueDictionary();
            var data = new RouteValueDictionary(htmlAttributes);
            foreach (var i in data)
            {
                htmlAtts.Add(i.Key.Replace("_", "-"), i.Value);
            }
            
            var extra = new RouteValueDictionary(routeValues);
            var m = Merge(r, extra);

            return helper.ActionLink(linktext, action, m, htmlAtts);
        }

        public static HtmlString RenderFile(this HtmlHelper helper, string file)
        {
            var fullPath = Context.Server.MapPath(file);
            return new HtmlString(File.ReadAllText(fullPath, new UTF8Encoding()));
        }

        public static HtmlString FileAsBase64(this HtmlHelper helper, string file, string contentType)
        {
            var fullPath = Context.Server.MapPath(file);
            var bytes = File.ReadAllBytes(fullPath);
            return new HtmlString("data:" + contentType + ";base64," + Convert.ToBase64String(bytes));
        }

        public static ModalWriter BeginModal(this HtmlHelper helper, string id, string title)
        {
            return helper.BeginModal(id, title, new { });
        }

        public static ModalWriter BeginModal(this HtmlHelper helper, string id, string title, object htmlAttributes)
        {
            return new ModalWriter(helper.ViewContext, id, title, htmlAttributes);
        }

        public static TemplateWriter BeginTemplate(this HtmlHelper helper, string id)
        {
            return helper.BeginTemplate(id, new { });
        }

        public static TemplateWriter BeginTemplate(this HtmlHelper helper, string id, object htmlAttributes)
        {
            return new TemplateWriter(helper.ViewContext, id, htmlAttributes);
        }
    }

    public sealed class TemplateWriter : IDisposable
    {
        private readonly ViewContext _viewContext;

        private readonly TagBuilder _builder;

        public TemplateWriter(ViewContext context, string id, object htmlAttributes)
        {
            var tb = new TagBuilder("script");
            tb.Attributes["type"] = "text/x-jquery-tmpl";
            tb.Attributes["id"] = id;

            var attributes = new RouteValueDictionary(htmlAttributes).ToDictionary(i => i.Key.Replace("_", "-"), i => i.Value);
            tb.MergeAttributes(attributes);

            _viewContext = context;
            _builder = tb;

            _viewContext.Writer.Write(_builder.ToString(TagRenderMode.StartTag));
        }

        public void Dispose()
        {
            _viewContext.Writer.Write(_builder.ToString(TagRenderMode.EndTag));
        }
    }

    public sealed class ModalWriter : IDisposable
    {
        private readonly ViewContext _viewContext;

        private readonly TagBuilder _builder;

        private readonly TagBuilder _container;

        private readonly TagBuilder _container2;

        public ModalWriter(ViewContext context, string id, string title, object htmlAttributes)
        {
            var tb = new TagBuilder("div");
            tb.Attributes["id"] = id;
            tb.Attributes["tabindex"] = "-1";
            tb.Attributes["role"] = "dialog";
            tb.Attributes["aria-labelledby"] = title;
            tb.Attributes["aria-hidden"] = "true";
            tb.Attributes["data-backdrop"] = "static";

            var attributes = new RouteValueDictionary(htmlAttributes).ToDictionary(i => i.Key.Replace("_", "-"), i => i.Value);
            var modalClass = "modal hide";
            if (attributes.ContainsKey("class"))
            {
                modalClass += " " + attributes["class"];
            }

            tb.Attributes["class"] = modalClass;
            tb.MergeAttributes(attributes);

            var button = new TagBuilder("button");
            button.Attributes["class"] = "close";
            button.Attributes["data-dismiss"] = "modal";
            button.Attributes["aria-hidden"] = "true";
            button.SetInnerText("x");

            var container = new TagBuilder("div");
            container.Attributes["class"] = "modal-body";

            var container2 = new TagBuilder("div");
            container2.Attributes["class"] = "modal-inner";

            _container = container;
            _container2 = container2;
            _viewContext = context;
            _builder = tb;
            _viewContext.Writer.Write(_builder.ToString(TagRenderMode.StartTag));
            _viewContext.Writer.Write(button.ToString(TagRenderMode.Normal));
            _viewContext.Writer.Write(_container.ToString(TagRenderMode.StartTag));
            _viewContext.Writer.Write(_container2.ToString(TagRenderMode.StartTag));
        }

        public void Dispose()
        {
            _viewContext.Writer.Write(_container2.ToString(TagRenderMode.EndTag));
            _viewContext.Writer.Write(_container.ToString(TagRenderMode.EndTag));
            _viewContext.Writer.Write(_builder.ToString(TagRenderMode.EndTag));
        }
    }
}