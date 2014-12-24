using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MooversCRM.Controllers
{
    public class TemplateController :  BaseControllers.SecureBaseController
    {
        //
        // GET: /Template/

      public ActionResult Index()
        {
            return View();
        }

    }
}
