using Business.Repository;
using Business.Repository.Models;
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
    using System.Web.Script.Serialization;

    [MenuDescription("Dashboard")]
    public class HomeController : BaseControllers.SecureBaseController
    {
        //[Authorize]
        [HttpGet]
        public ActionResult Index(string error = "")
        {
            var msgRepo = new FrontPageMessageRepository();
            var msgs = msgRepo.GetLatest();

            if (!String.IsNullOrEmpty(error))
            {
                ModelState.AddModelError("search", error);
            }

            var quoteRepo = new QuoteRepository();
            var quotes = quoteRepo.GetLastAccessed(AspUserID);

            var scheduled = new ScheduleRepository();
            var todays = scheduled.GetForDay(SessionFranchiseID, DateTime.Today).Select(i => i.Quote).Distinct();

            var surveyRepo = new QuoteSurveyRepository();
            var surveys = surveyRepo.GetForDay(SessionFranchiseID, DateTime.Today);


            var franchiseRepo = new FranchiseRepository();
            var franchise = (AspUser.HasMultipleFranchises()) ? franchiseRepo.GetStorage() :  AspUser.GetSingleFranchise();

            var model = new HomePageViewModel(msgs, quotes, surveys, todays, franchise.Address);
            return View(model);
        }

        [HttpGet]
        public JsonResult ScheduleTodayJson()
        {
            var scheduled = new ScheduleRepository();
            var todays = scheduled.GetForDay(SessionFranchiseID, DateTime.Today.AddMonths(-2)).Select(i => i.Quote).Distinct().Select(m=>m.ToJsonObject());
            return Json(todays, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult QuoteTodayJson()
        {
            var quoteRepo = new QuoteRepository();
            var quotes = quoteRepo.GetLastAccessed(AspUserID).Select(m=>m.ToJsonObject()).ToList();
            //var str = new JavaScriptSerializer().Serialize(quotes);
            return Json(quotes, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult SurveyTodayJson()
        {
            var surveyRepo = new QuoteSurveyRepository();
            var surveys = surveyRepo.GetForDay(SessionFranchiseID, DateTime.Today);
            return Json(surveys, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public JsonResult MessagesJson()
        {
            var msgRepo = new FrontPageMessageRepository();
            var msgs = msgRepo.GetLatest();
            return Json(msgs.Select(m=>new{date = m.Date, user=m.aspnet_Users.UserName,text=m.Message }), JsonRequestBehavior.AllowGet);

        }
        public ActionResult RemoveMsg(Guid id)
        {
            var repo = new FrontPageMessageRepository();
            var msg = repo.Get(id);

            if (msg != null)
            {
                repo.Remove(msg);
                repo.Save();
            }

            return RedirectToAction("Index");
        }

        public ActionResult KeepAlive()
        {
            using (var entity = new Business.Models.MooversCRMEntities())
            {
                return Json(entity.DatabaseExists(), JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult AddMessage(string addMessage)
        {
            var repo = new FrontPageMessageRepository();

            var msg = new Business.Models.FrontPageMessage {
                Message = addMessage,
                UserID = AspUserID,
                Date = DateTime.Now
            };

            repo.Add(msg);
            repo.Save();
            return RedirectToAction("Index");
        }

        [ActionName("Index")]
        [HttpPost]
        public ActionResult Index_Post(string search)
        {
            search = search.ToLower();
            if (search.StartsWith("a"))
            {
                var repo = new AccountRepository();
                var acct = repo.Get(search);

                if (acct == null)
                {
                    return RedirectToAction("Index", new { error = "Account not found" });
                }

                return RedirectToAction("Index", new { id = acct.Lookup, Controller = "Accounts" });
            }
            else
            {
                var repo = new QuoteRepository();
                var quote = repo.Get(search);
                if (quote == null)
                {
                    return RedirectToAction("Index", new { error = "Quote not found" });
                }

                return RedirectToAction("Stops", new { id = quote.Lookup, Controller = "Quote" });
            }
        }

        [Authorize]
        public ActionResult SetFranchiseID(Guid franchiseid, string redirect)
        {
            if (AspUser.HasPermissionsOn(franchiseid))
            {
                this.SessionFranchiseID = franchiseid;
            }

            return Redirect(redirect);
        }

        public ActionResult InventoryList()
        {
            var repo = new InventoryItemRepository();
            return View(repo.GetUnarchived());
        }

        [Authorize]
        [HttpGet]
        public ActionResult Settings(bool success = false)
        {
            var repo = new aspnet_UserRepository();
            var user = repo.Get(User.Identity.Name);
            ViewBag.Success = success;
            return View(new AccountEditModel(user));
        }

        [Authorize]
        public ActionResult EditSettings()
        {
            var repo = new aspnet_UserRepository();
            var user = repo.Get(User.Identity.Name);
            return View(new AccountEditModel(user));
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditSettings(string email, string oldpassword, string password, string confirmpassword)
        {
            var user = System.Web.Security.Membership.GetUser();
            var repo = new aspnet_UserRepository();

            if (password != confirmpassword)
            {
                ModelState.AddModelError("password", "Please type the same password twice");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!String.IsNullOrWhiteSpace(password))
                    {
                        if (!user.ChangePassword(oldpassword, password))
                        {
                            ModelState.AddModelError("oldpassword", "Please enter your current password");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    ModelState.AddModelError("password", "Please ensure your password uses a letter, a number, and a symbol");
                }

                if (email != user.Email)
                {
                    if (repo.GetByEmail(email) != null)
                    {
                        ModelState.AddModelError("email", "Please choose an e-mail address that's not in use");
                    }
                    else
                    {
                        try
                        {
                            user.Email = email;
                            Membership.UpdateUser(user);
                        }
                        catch (Exception)
                        {
                            ModelState.AddModelError("email", "Please enter a valid e-mail");
                        }
                    }
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction("Settings", new { success = "true" });
            }

            return View();
        }
    }
}
