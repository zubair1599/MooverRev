using Business.Enums;
using Business.Repository;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;
using Moovers.WebModels;
using MooversCRM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MooversCRM.Controllers
{
    //[Authorize]
    [MenuDescription("Leads")]
    public class LeadController : BaseControllers.SecureBaseController
    {
       
        [HttpGet]
        
        public JsonResult GetLeadCount(string franchiseID =null)
        {
            // corporate user
            if (franchiseID==null)
            {
                var repo = new LeadRepository();
                var quoteRepo = new QuoteRepository();
                var leadCount = repo.GetUnreadCount();
                var webCount = quoteRepo.GetOpenForUser(this.WebQuoteUserID).Count();
                return Json(leadCount + webCount,JsonRequestBehavior.AllowGet);
            }
            else
            {
                var repo = new LeadRepository();
                var quoteRepo = new QuoteRepository();
                var leadCount = repo.GetUnreadCount(new Guid(franchiseID));
                var webCount = quoteRepo.GetOpenForUser(new Guid(franchiseID), this.WebQuoteUserID).Count();
                return Json(leadCount + webCount,JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult List()
        {
            Guid? franchiseid = null;
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseid = SessionFranchiseID;
            }

            var repo = new LeadRepository();
            var leads = repo.GetAll(franchiseid);
         
            var quoteRepo = new QuoteRepository();
            var quotes = (franchiseid.HasValue) ? quoteRepo.GetOpenForUser(franchiseid.Value, WebQuoteUserID) : quoteRepo.GetOpenForUser(WebQuoteUserID);
            foreach (var quote in quotes)
            {
                if (quote.PricingType == QuotePricingType.Binding && quote.GuaranteeData.BasePrice == default(decimal))
                {
                    quote.AddLog(WebQuoteUserID, "Webquote price generated");
                    quoteRepo.UpdateGuaranteedPrice(quote.QuoteID);
                }
            }

            quoteRepo.Save();

            return View(new LeadListModel(leads, quotes));
        }

        public ActionResult SmartMoves()
        {
            var smartrepo = new SmartMoveRepository();
            var smartmoves = smartrepo.GetAll();
            return View(new SmartListModel(smartmoves));
        }

        public ActionResult View(Guid id)
        {
            var repo = new LeadRepository();
            var lead = repo.Get(id);
            return View(lead);
        }

        public ActionResult ViewSmartMove(Guid id)
        {
            var repo = new SmartMoveRepository();
            var smartmove = repo.Get(id);
            return View(smartmove);
        }

        [HttpPost]
        public ActionResult RemoveChecked(Guid[] removeids)
        {
            var repo = new LeadRepository();
            var leads = removeids.Select(i => repo.Get(i));
            foreach (var l in leads)
            {
                l.IsArchived = true;
            }

            repo.Save();
            return RedirectToAction("List");
        }

        public ActionResult AcceptLead(Guid id)
        {
            var repo = new LeadRepository();
            var lead = repo.Get(id);
            lead.AccountManagerID = this.AspUserID;
            repo.Save();

            return RedirectToAction("View","Lead", new { id = id});
        }
        public ActionResult ToAccount(Guid id)
        {
            var repo = new LeadRepository();
            var lead = repo.Get(id);
            var accountRepo = new AccountRepository();
            var account = lead.GetLeadJson().GetAccount();

            account.FranchiseID = lead.FranchiseID ?? new FranchiseRepository().GetStorage().FranchiseID;

            accountRepo.Add(account);
            accountRepo.Save();
            lead.AccountID = account.AccountID;
            lead.IsArchived = true;
            repo.Save();

            var lookup = new AccountRepository().Get(account.AccountID).Lookup;
            return RedirectToAction("Index", new { Controller = "Accounts", id = lookup });
        }
        public ActionResult LinkAccount(Guid accountid,string lookUp, Guid leadid)
        {
            var leadrepo = new LeadRepository();
            var lead = leadrepo.Get(leadid);
            lead.AccountID = accountid;
            lead.IsArchived = true;
            leadrepo.Save();

            return RedirectToAction("Index", new { Controller = "Accounts", id = lookUp });
        }
    }
}
