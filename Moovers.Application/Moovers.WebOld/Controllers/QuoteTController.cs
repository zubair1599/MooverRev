using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MooversCRM.Controllers
{
    using MooversCRM.Controllers.BaseControllers;

    public class QuoteTController : SecureBaseController
    {
        //
        // GET: /QuoteT/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }
    }
}
