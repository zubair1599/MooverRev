// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="AccountsController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Business.JsonObjects;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;
    using Business.ViewModels;

    using FluentValidation.Results;

    using Moovers.WebModels;
    using Moovers.WebModels.Validators;

    using MooversCRM.Controllers.BaseControllers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    //[Authorize]
    public class AccountsController : SecureBaseController
    {
        [HttpGet]
        public ActionResult Index(string id = null)
        {
            Guid? accountID = null;
            if (id != null)
            {
                var repo = new AccountRepository();
                Account account = repo.Get(id);
                if (account != null)
                {
                    accountID = account.AccountID;
                }
            }

            return View(accountID);
        }

        [HttpGet]
        public JsonResult All(string q = null, int page = 0, int take = 50)
        {
            Guid? franchiseid = null;
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseid = SessionFranchiseID;
            }

            var repo = new AccountRepository();
            PagedResult<SmallAccountJson> results = repo.Search(franchiseid, q, page, take);
            return Json(results.Items, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMatchingLeadAccount(Guid id)
        {
            var leadRepo = new LeadRepository();
            Lead lead = leadRepo.Get(id);
            LeadJson leadjson = lead.GetLeadJson();
            Guid? franchiseid = null;
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseid = SessionFranchiseID;
            }

            var repo = new AccountRepository();
            PagedResult<SmallAccountJson> results = repo.Search(franchiseid, lead.Name, 0, 2000);

            if (!string.IsNullOrEmpty(leadjson.HomePhone))
            {
                results.ToList().AddRange(repo.Search(franchiseid, leadjson.HomePhone, 0, 20));
            }
            if (!string.IsNullOrEmpty(leadjson.MobilePhone))
            {
                results.ToList().AddRange(repo.Search(franchiseid, leadjson.HomePhone, 0, 20));
            }
            if (!string.IsNullOrEmpty(leadjson.WorkPhone))
            {
                results.ToList().AddRange(repo.Search(franchiseid, leadjson.HomePhone, 0, 20));
            }
            if (!string.IsNullOrEmpty(leadjson.Email))
            {
                results.ToList().AddRange(repo.Search(franchiseid, leadjson.Email, 0, 20));
            }

            var accounts = new List<Account>();
            var accrepo = new AccountRepository();
            results.ToList().ForEach(a => { accounts.Add(accrepo.Get(a.AccountID)); });
            @ViewBag.LeadID = lead.LeadID;
            return PartialView("_MatchingAccounts", accounts);
        }

        public ActionResult Get(Guid id)
        {
            var repo = new AccountRepository();
            Account account = repo.Get(id);

            if (account == null)
            {
                return HttpNotFound();
            }

            return Json(account.ToJsonObject(), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetFromQuote(string Id)
        {
            var repo = new AccountRepository();

            Account account = repo.GetAccountFromQuote(Id.ToString());

            if (account == null)
            {
                return HttpNotFound();
            }

            return Json(account.ToJsonObject(), JsonRequestBehavior.AllowGet);
        }

        // POST: /Accounts/Person/Add
        [HttpPost]
        public ActionResult CreatePerson(Guid? accountid, FormCollection coll, PersonAccountModel model, bool currentMailing = false)
        {
            var repo = new PersonAccountRepository();
            PersonAccount account = (accountid.HasValue) ? repo.Get(accountid.Value) : new PersonAccount();
            var validator = new AccountModelValidator<PersonAccount>();

            if (!accountid.HasValue)
            {
                repo.Add(account);
            }

            model.UpdateAddresses(account, coll, currentMailing);
            ValidationResult validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                return Json(new ErrorModel(validation));
            }

            repo.UpdateFromForm(SessionFranchiseID, account, model);
            repo.Save();
            return Content(LocalExtensions.SerializeToJson(account.ToJsonObject()), "text/json");
        }

        [HttpPost]
        public ActionResult CreatePerson2(Guid? accountid, PersonAccountModel model)
        {
            var repo = new PersonAccountRepository();
            PersonAccount account = (accountid.HasValue) ? repo.Get(accountid.Value) : new PersonAccount();
            var validator = new AccountModelValidator<PersonAccount>();

            if (!accountid.HasValue)
            {
                repo.Add(account);
            }

            //model.UpdateAddresses(account, coll, currentMailing);
            ValidationResult validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                return Json(new ErrorModel(validation));
            }

            repo.UpdateFromForm(SessionFranchiseID, account, model);
            repo.Save();
            return Content(LocalExtensions.SerializeToJson(account.ToJsonObject()), "text/json");
        }

        public ActionResult CreateBusiness2(Guid? accountid, BusinessAccountModel model)
        {
            var repo = new BusinessAccountRepository();
            BusinessAccount account = (accountid.HasValue) ? repo.Get(accountid.Value) : new BusinessAccount();
            //model.Account.Lookup = null;
            if (!accountid.HasValue)
            {
                repo.Add(account);
            }

            //model.UpdateAddresses(account, coll, false);
            var validator = new AccountModelValidator<BusinessAccount>();
            ValidationResult validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                return Json(new ErrorModel(validation));
            }

            repo.UpdateFromForm(SessionFranchiseID, account, model);
            repo.Save();
            return Content(LocalExtensions.SerializeToJson(account.ToJsonObject()), "text/json");
        }

        // POST: /Accounts/Business/Add
        [HttpPost]
        public ActionResult CreateBusiness(Guid? accountid, FormCollection coll, BusinessAccountModel model)
        {
            var repo = new BusinessAccountRepository();
            BusinessAccount account = (accountid.HasValue) ? repo.Get(accountid.Value) : new BusinessAccount();

            if (!accountid.HasValue)
            {
                repo.Add(account);
            }

            model.UpdateAddresses(account, coll, false);
            var validator = new AccountModelValidator<BusinessAccount>();
            ValidationResult validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                return Json(new ErrorModel(validation));
            }

            repo.UpdateFromForm(SessionFranchiseID, account, model);
            repo.Save();
            return Content(LocalExtensions.SerializeToJson(account.ToJsonObject()), "text/json");
        }
        //[Bind(Exclude = "Salary")]
      

        [HttpGet]
        public JsonResult CreateNewBusiness()
        {
            var t =  new BusinessAccountModel()
            {
                MailingAddress =  new Address(),
                BillingAddress = new Address(),
                PrimaryEmail =  new EmailAddress(),
                SecondaryEmail =  new EmailAddress(),
                FaxPhone = new PhoneNumber(),
                PrimaryPhone = new PhoneNumber(),
                SecondaryPhone =  new PhoneNumber(),
                Account = new BusinessAccount()
                

            }.SerializeToJson(2);
            return Json(t, JsonRequestBehavior.AllowGet);

        }
        public JsonResult CreateNewPerson()
        {
            var t = new PersonAccountModel()
            {
                MailingAddress = new Address(),
                BillingAddress = new Address(),
                PrimaryEmail = new EmailAddress(),
                SecondaryEmail = new EmailAddress(),
                FaxPhone = new PhoneNumber(),
                PrimaryPhone = new PhoneNumber(),
                SecondaryPhone = new PhoneNumber(),
                
                Account = new PersonAccount()


            }.SerializeToJson(2);
            return Json(t, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult CreateBusinessJson(Guid? accountid, string accountsJson)
        {
            var model = new BusinessAccountModel();
            var addressDetails = (JObject) JsonConvert.DeserializeObject(accountsJson);

            var repo = new BusinessAccountRepository();
            BusinessAccount account = (accountid.HasValue) ? repo.Get(accountid.Value) : new BusinessAccount();

            if (!accountid.HasValue)
            {
                repo.Add(account);
            }

            model.UpdateAddresses(account, accountsJson, false);
            var validator = new AccountModelValidator<BusinessAccount>();
            ValidationResult validation = validator.Validate(model);
            if (!validation.IsValid)
            {
                return Json(new ErrorModel(validation));
            }

            repo.UpdateFromForm(SessionFranchiseID, account, model);
            repo.Save();
            return Content(LocalExtensions.SerializeToJson(account.ToJsonObject()), "text/json");
        }
        public ActionResult ManageAccounts()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MergeAccounts(string acct1, string acct2)
        {
            var repo = new AccountRepository();
            Account account1 = repo.Get(acct1);
            Account account2 = repo.Get(acct2);

            List<Quote> quotes = account2.BillingQuotes.Concat(account2.ShippingQuotes).Distinct().ToList();
            foreach (Quote q in quotes)
            {
                q.AccountID = q.ShippingAccountID = account1.AccountID;
            }

            account2.IsArchived = true;
            repo.Save();

            return RedirectToAction("ManageAccounts");
        }

        [HttpPost]
        public ActionResult ArchiveAccount(string accountid)
        {
            var repo = new AccountRepository();
            Account account = repo.Get(accountid);
            account.IsArchived = true;
            repo.Save();

            return RedirectToAction("ManageAccounts");
        }
    }
}