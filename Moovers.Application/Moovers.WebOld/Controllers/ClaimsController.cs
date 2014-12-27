using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.ViewModels;

namespace MooversCRM.Controllers
{
    public class ClaimsController : BaseControllers.SecureBaseController
    {
        //
        // GET: /Claims/

        public ActionResult Index(Business.ViewModels.ClaimSort sort = Business.ViewModels.ClaimSort.Created, bool desc = false, bool status = true)
        {
            var repo = new Business.Models.ClaimRepository();
            var claims = repo.GetClaim(status); //(Status) ? repo.GetClosedClaim(Status) : repo.GetOpenClaim(Status.Open.ToString());
            ViewBag.status = status;
           
            return View(new Business.ViewModels.ClaimListModel(claims, sort, desc));
        }

    }
}
