// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="PublicController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;

    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;

    using MooversCRM.Controllers.BaseControllers;

    public class PublicController : NonSecureBaseController
    {
        public ActionResult Browser()
        {
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult NotAuthorized()
        {
            return View();
        }

        public ActionResult Error404()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            return View();
        }

        public ActionResult ConfirmMove(string id)
        {
            var confirmRepo = new ScheduleConfirmationRepository();
            ScheduleConfirmation confirm = confirmRepo.Get(id);

            if (confirm != null)
            {
                ViewBag.Key = id;
                return View(confirm);
            }

            if (!String.IsNullOrEmpty(id))
            {
                ViewBag.ErrorMessage = "Your confirmation code wasn't found, please make sure you typed it correctly, or call us at 1-800-MOOVERS";
            }

            return View();
        }

        public ActionResult ProposalView(string id)
        {
            var repo = new ScheduleConfirmationRepository();
            ScheduleConfirmation confirmation = repo.Get(id);
            File file = confirmation.Quote.GetFiles().Where(i => i.Name.Contains("Proposal")).OrderByDescending(i => i.Created).FirstOrDefault();

            if (file != null)
            {
                return File(file.SavedContent, file.ContentType);
            }

            return HttpNotFound();
        }

        public ActionResult EmailView(string id)
        {
            var repo = new ScheduleConfirmationRepository();
            ScheduleConfirmation confirm = repo.Get(id);

            if (confirm == null)
            {
                return HttpNotFound();
            }

            ViewBag.ShowEmailLink = false;
            return Content(RenderViewToString("Emails/MoveConfirmation", confirm), "text/html");
        }

        [HttpPost]
        public ActionResult Confirm(string key)
        {
            if (!String.IsNullOrEmpty(key))
            {
                var confirmRepo = new ScheduleConfirmationRepository();
                ScheduleConfirmation confirm = confirmRepo.Get(key);
                var repo = new QuoteRepository();
                Quote quote = repo.Get(confirm.QuoteID);
                quote.ConfirmQuote("Email Form", Request["REMOTE_ADDR"], Request["HTTP_USER_AGENT"]);
                repo.Save();
                return RedirectToAction("ConfirmMove", new { id = key });
            }

            return HttpNotFound();
        }

        public ActionResult Unsubscribe(string email)
        {
            email = email.Trim().Replace("~", ".");
            if (String.IsNullOrEmpty("email"))
            {
                return HttpNotFound();
            }

            var repo = new EmailUnsubscribeRepository();
            if (!repo.Exists(email))
            {
                var unsubscribe = new EmailUnsubscribe(email, Request.ServerVariables["REMOTE_ADDR"]);
                repo.Add(unsubscribe);
                repo.Save();
            }

            return View((object)email);
        }
    }
}