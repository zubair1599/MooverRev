using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MooversCRM.Controllers
{
    using MooversCRM.Controllers.BaseControllers;

    public class NewController : SecureBaseController
    {
        //
        // GET: /New/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Quote()
        {
            return View();

        }

}
}
