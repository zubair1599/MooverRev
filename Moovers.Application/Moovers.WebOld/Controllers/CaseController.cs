using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.Enums;
using Business.Repository.Models;
using Business.ViewModels;
using Moovers.WebModels;
using MooversCRM.Attributes;

namespace MooversCRM.Controllers
{
    [MenuDescription("Cases")]
    public class CaseController : BaseControllers.SecureBaseController
    {
        //
        // GET: /Case/

        public ActionResult Index(CaseSort sort = CaseSort.Created, bool desc = false, bool status = true)
        {
            var repo = new CaseRepository();
            var claims = (status) ? repo.GetCase() : repo.GetClosedClaim();
            ViewBag.status = status;
           
            return View(new CaseListModel(claims, sort, desc));
        }
        [HttpPost]
        public ActionResult AddRemarks(Guid id, string remarks, string casestatus, int ddlcasestatus)
        {
            var caserepo = new CaseRepository();
            var  cse =  caserepo.Get(id);
            cse.Remarks = remarks;
            cse.Status = ddlcasestatus;
            if (ddlcasestatus == (int)CaseStatus.Closed)
            {
                cse.CaseCloseDate = DateTime.Now;
                cse.Updated = DateTime.Now;
                caserepo.Save();
                var ToEmail = cse.Quote.Account.GetEmail(Business.Models.EmailAddressType.Primary);
                var subject = "Your Case (" + cse.Lookup + ") has been closed" ;
                Business.Utility.Email.SendEmail(ToEmail.Email, "support@1800moovers.com", subject, remarks);
            }
            else
            {
                cse.CaseReOpenDate = DateTime.Now;
                cse.Updated = DateTime.Now;
                caserepo.Save();
            }
            

            return RedirectToAction("Index", new { status = casestatus});
        }
    }
}
