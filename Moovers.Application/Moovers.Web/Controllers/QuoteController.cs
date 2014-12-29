// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="QuoteController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

using CardTypes = Business.FirstData.FirstDataCardTypes;

namespace MooversCRM.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Script.Serialization;
    using System.Web.Security;
    using System.Web.UI;

    using Business.Enums;
    using Business.FirstData;
    using Business.JsonObjects;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.ToClean;
    using Business.ToClean.QuoteHelpers;
    using Business.Utility;
    using Business.ViewModels;

    using FluentSecurity;

    using FluentValidation.Results;

    using Moovers.WebModels;
    using Moovers.WebModels.Validators;

    using MooversCRM.Attributes;
    using MooversCRM.Controllers.BaseControllers;

    //[Authorize]
    [MenuDescription("Quotes")]
    public class QuoteController : SecureBaseController
    {
        public ActionResult GetStats(string search)
        {
            Guid? franchiseID = null;
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseID = AspUser.GetSingleFranchise().FranchiseID;
            }

            var repo = new QuoteRepository();
            return Json(repo.GetCumulativeStats(franchiseID, search), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(string search = "", QuoteSort sort = QuoteSort.LastModified, int page = 0, bool desc = true, int take = 25)
        {
            var repo = new QuoteRepository();

            Guid? franchiseID = null;
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseID = AspUser.GetSingleFranchise().FranchiseID;
            }

            if (Request.IsAjaxRequest() || !String.IsNullOrEmpty(search))
            {
                IQueryable<Quote> quotes = repo.GetSearch(franchiseID, SearchParser.Parse(search), sort, desc);

                var items = new PagedResult<Quote>(quotes, page, take);
                var model = new QuoteListModel(items, sort, desc);

                if (Request.IsAjaxRequest())
                {
                    return Json(model.ToJsonObject(), JsonRequestBehavior.AllowGet);
                }
                if (items.TotalCount == 1)
                {
                    return RedirectToAction("Overview", "Quote", new { id = items.First().Lookup });
                }
            }

            ViewBag.Search = search;
            return View(new QuoteListModel(new PagedResult<Quote>(Enumerable.Empty<Quote>().AsQueryable(), page, take), sort, desc));
        }

        public ActionResult GetStorageAccounts(string search = "")
        {
            if (String.IsNullOrEmpty(search))
            {
                return Json(new string[0]);
            }

            var repo = new StorageWorkOrderRepository();
            List<StorageWorkOrder> accounts = repo.Search(search).ToList();
            return Json(accounts.Select(i => i.ToJsonObject()));
        }

        public ActionResult ChangePricing(Guid quoteid, decimal? firsthour, decimal? extrahour)
        {
            if (!IsAdministrator)
            {
                throw new HttpException(403, "No permission");
            }

            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);
            if (firsthour.HasValue && extrahour.HasValue)
            {
                quote.HourlyData = new HourlyInfo()
                {
                    CustomerTimeEstimate = quote.HourlyData.CustomerTimeEstimate,
                    FirstHourPrice = firsthour.Value * (quote.Trucks ?? 1),
                    HourlyPrice = extrahour.Value * (quote.Trucks ?? 1)
                };

                quote.CustomHourlyRate = extrahour.Value;
            }

            repo.Save();

            return RedirectToAction("Pricing", new { id = quote.Lookup });
        }

        // POST /Quote/AddQuote
        [HttpPost]
        public ActionResult AddQuote(
            string shippingAccount,
            Guid hiddenAccountID,
            string movedate,
            string referralmethod,
            PersonAccountModel personModel,
            BusinessAccountModel businessModel,
            FormCollection coll)
        {
            var accountTypes = new { billing = "--SAME-AS-BILLING--", person = "--NEW PERSON--", business = "--NEW BUSINESS--" };

            Guid shippingAccountID;
            if (shippingAccount == accountTypes.person)
            {
                // add a person, use the person as account
                var personRepo = new PersonAccountRepository();
                var account = new PersonAccount();

                var validator = new AccountModelValidator<PersonAccount>();
                personModel.UpdateAddresses(account, coll, false);

                ValidationResult validationResults = validator.Validate(personModel);
                if (!validationResults.IsValid)
                {
                    return Json(new ErrorModel(validationResults));
                }

                personRepo.Add(account);
                personRepo.UpdateFromForm(account.FranchiseID, account, personModel);
                personRepo.Save();

                shippingAccountID = account.AccountID;
            }
            else if (shippingAccount == accountTypes.business)
            {
                // add a business, use the business as account
                var businessRepo = new BusinessAccountRepository();
                var account = new BusinessAccount();
                businessModel.UpdateAddresses(account, coll, false);

                var validator = new AccountModelValidator<BusinessAccount>();
                ValidationResult validationResults = validator.Validate(businessModel);
                if (!validationResults.IsValid)
                {
                    return Json(new ErrorModel(validationResults));
                }

                businessRepo.Add(account);
                businessRepo.UpdateFromForm(account.FranchiseID, account, businessModel);
                businessRepo.Save();
                shippingAccountID = account.AccountID;
            }
            else
            {
                // if (shippingAccount == accountTypes.billing)
                // same as billing account
                shippingAccountID = hiddenAccountID;
            }

            var repo = new QuoteRepository();

            var quote = new Quote
            {
                AccountID = hiddenAccountID,
                ShippingAccountID = shippingAccountID,
                MoveDate = DateTime.Parse(movedate),
                ReferralMethod = referralmethod,
                AccountManagerID = AspUserID,
                FranchiseID = SessionFranchiseID,
                GuaranteeData = new GuaranteedInfo() { Adjustments = 0, BasePrice = 0, GuaranteedPrice = 0 }
            };

            // if a franchisee creates a quote, always associate that quote with their franchise
            if (IsFranchiseUser)
            {
                quote.ForceFranchiseID = SessionFranchiseID;
            }

            repo.Add(quote);
            repo.Save();

            //Default Valuation Type for New Quote
            ReplacementValuation replacementvaluation = new ReplacementValuationRepository().GetAll().FirstOrDefault(model => model.Type == 1);
            if (replacementvaluation != null)
            {
                quote.ValuationTypeID = replacementvaluation.ValuationTypeID;
                repo.Save();
            }

            string lookup = repo.Get(quote.QuoteID).Lookup;
            string redirectUrl = Url.Action("Stops", new { id = lookup });
            if (Request.IsAjaxRequest())
            {
                return Json(new { redirect = redirectUrl });
            }

            return Redirect(redirectUrl);
        }



        [HttpPost]
        public JsonResult AddQuoteJson(
            string shippingAccount,
            Guid hiddenAccountID,
            string movedate,
            string referralmethod, FormCollection coll = null,
            PersonAccountModel personModel=null,
            BusinessAccountModel businessModel = null
           )
        {
            var accountTypes = new { billing = "--SAME-AS-BILLING--", person = "--NEW PERSON--", business = "--NEW BUSINESS--" };

            Guid shippingAccountID;
            if (shippingAccount == accountTypes.person)
            {
                // add a person, use the person as account
                var personRepo = new PersonAccountRepository();
                var account = new PersonAccount();

                var validator = new AccountModelValidator<PersonAccount>();
                personModel.UpdateAddresses(account, coll, false);

                ValidationResult validationResults = validator.Validate(personModel);
                if (!validationResults.IsValid)
                {
                    return Json(new ErrorModel(validationResults));
                }

                personRepo.Add(account);
                personRepo.UpdateFromForm(account.FranchiseID, account, personModel);
                personRepo.Save();

                shippingAccountID = account.AccountID;
            }
            else if (shippingAccount == accountTypes.business)
            {
                // add a business, use the business as account
                var businessRepo = new BusinessAccountRepository();
                var account = new BusinessAccount();
                businessModel.UpdateAddresses(account, coll, false);

                var validator = new AccountModelValidator<BusinessAccount>();
                ValidationResult validationResults = validator.Validate(businessModel);
                if (!validationResults.IsValid)
                {
                    return Json(new ErrorModel(validationResults));
                }

                businessRepo.Add(account);
                businessRepo.UpdateFromForm(account.FranchiseID, account, businessModel);
                businessRepo.Save();
                shippingAccountID = account.AccountID;
            }
            else
            {
                // if (shippingAccount == accountTypes.billing)
                // same as billing account
                shippingAccountID = hiddenAccountID;
            }

            var repo = new QuoteRepository();

            var quote = new Quote
            {
                AccountID = hiddenAccountID,
                ShippingAccountID = shippingAccountID,
                MoveDate = DateTime.Parse(movedate),
                ReferralMethod = referralmethod,
                AccountManagerID = AspUserID,
                FranchiseID = SessionFranchiseID,
                GuaranteeData = new GuaranteedInfo() { Adjustments = 0, BasePrice = 0, GuaranteedPrice = 0 }
            };

            // if a franchisee creates a quote, always associate that quote with their franchise
            if (IsFranchiseUser)
            {
                quote.ForceFranchiseID = SessionFranchiseID;
            }

            repo.Add(quote);
            repo.Save();

            //Default Valuation Type for New Quote
            ReplacementValuation replacementvaluation = new ReplacementValuationRepository().GetAll().FirstOrDefault(model => model.Type == 1);
            if (replacementvaluation != null)
            {
                quote.ValuationTypeID = replacementvaluation.ValuationTypeID;
                repo.Save();
            }

            string lookup = repo.Get(quote.QuoteID).Lookup;
            string redirectUrl = Url.Action("Stops", new { id = lookup });

            return Json(new { quote = quote.ToJsonObject(true) }, JsonRequestBehavior.AllowGet);

          
        }
        [HttpGet]
        public ActionResult GetRecentQuoteJson(string lookup)
        {
            var repo = new QuoteRepository();

            var quote = repo.Get(lookup);
           
            //string redirectUrl = Url.Action("Stops", new { id = quote.Lookup });
            
            return Json(new {quote= quote.ToJsonObject(true) },JsonRequestBehavior.AllowGet);
        }



        // POST /Quote/EditComment
        [HttpPost]
        public ActionResult EditComment(Guid commentid, string text, bool delete = false)
        {
            var repo = new QuoteCommentRepository();
            QuoteComment comment = repo.Get(commentid);

            if (comment == null || (!IsAdministrator && !comment.IsEditable()))
            {
                return HttpNotFound();
            }

            if (delete)
            {
                repo.Delete(comment);
                repo.Save();
                return Json(true);
            }

            comment.Text = text;
            repo.Save();
            return Json(comment.ToJsonObject());
        }

        [HttpPost]
        public ActionResult AddCustomItem(string name, decimal cubicFeet, int moversRequired)
        {
            if (String.IsNullOrEmpty(name))
            {
                return Json(new ErrorModel("name", "Please enter a name"));
            }

            if (cubicFeet <= 0)
            {
                return Json(new ErrorModel("cubicfeet", "Please enter an approximate size"));
            }

            if (moversRequired <= 0)
            {
                return Json(new ErrorModel("moversRequired", "Please enter approximate # of movers required"));
            }

            var repo = new InventoryItemRepository();

            var item = new InventoryItem
            {
                Name = name,
                PluralName = name,
                CubicFeet = cubicFeet,
                Weight = cubicFeet * 7,
                MoversRequired = moversRequired,
                IsCustom = true
            };

            repo.Add(item);
            repo.Save();
            return Json(repo.Get(item.ItemID).ToJsonObject());
        }

        [HttpPost]
        public ActionResult ChangeStatus(Guid id, string action, string reason, string redirect)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (action == "Reopen")
            {
                if (quote.Status == QuoteStatus.Completed)
                {
                    return new HttpStatusCodeResult(403, "Can't re-open a completed move.");
                }

                quote.SetStatus(AspUserID, QuoteStatus.Open, reason);
                repo.Save();
                return Redirect(redirect);
            }

            string redirUrl = redirect;

            if (quote.Status != QuoteStatus.Open)
            {
                VerifyWrite(quote);
            }

            quote.CancelSchedules(AspUserID);
            quote.ActionReason = reason;

            if (action != "Duplicate")
            {
                repo.UpdateGuaranteedPrice(quote.QuoteID);
            }

            if (action == "Defer")
            {
                quote.SetStatus(AspUserID, QuoteStatus.Deferred, reason);
            }

            if (action == "Reschedule")
            {
                quote.CancellationDate = DateTime.Now;
                quote.SetStatus(AspUserID, QuoteStatus.Open, reason);
                redirUrl = Url.Action("Schedule", new { id = quote.Lookup });
            }

            if (action == "Cancel")
            {
                quote.CancellationDate = DateTime.Now;
                quote.SetStatus(AspUserID, QuoteStatus.Cancelled, reason);
            }

            if (action == "Lost" || action == "Close")
            {
                quote.CancellationDate = DateTime.Now;
                quote.SetStatus(AspUserID, QuoteStatus.Lost, reason);
                if (action == "Lost")
                {
                    quote.QuoteSurveys.ToList().ForEach(qs => { qs.IsCancelled = true; });
                }
            }

            if (action == "Duplicate")
            {
                quote.SetStatus(AspUserID, QuoteStatus.Duplicate, reason);
            }

            repo.Save();
            return Redirect(redirUrl);
        }

        [HttpPost]
        public ActionResult AddStorage(string id, int days)
        {
            var quoteRepo = new QuoteRepository();

            Quote quote = quoteRepo.Get(id);
            if (!quote.CanUserEdit(User.Identity.Name))
            {
                return HttpNotFound();
            }

            Stop stop = Stop.GetStorageStop(quote.Franchise, quote.GetStops().Any() ? quote.GetStops().Max(i => i.Sort) + 1 : 0);
            stop.StorageDays = days;

            quote.Stops.Add(stop);
            quoteRepo.Save();

            return RedirectToAction("Stops", new { id = quote.Lookup, Controller = "Quote" });
        }

        [HttpGet]
        public ActionResult Clone(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                Quote dupe = quote.Duplicate(AspUserID);
                repo.Add(dupe);
                repo.Save();
                return RedirectToAction("Stops", new { id = dupe.Lookup });
            }

            return HttpNotFound();
        }

        public ActionResult CloneMoveOut(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                Quote dupe = quote.Duplicate(AspUserID, true);
                repo.Add(dupe);
                repo.Save();

                // remove all items from storage after we 'move out'
                foreach (Room_InventoryItem inv in dupe.Stops.SelectMany(i => i.Rooms.SelectMany(r => r.Room_InventoryItems)))
                {
                    inv.StorageCount = 0;
                }

                repo.Save();

                return RedirectToAction("Stops", new { id = dupe.Lookup });
            }

            return HttpNotFound();
        }

        /// <summary>
        ///     Accessed by jquery.fullcalendar to get quote listing for each day
        ///     GET /Quote/GetSchedule
        ///     NOTE: This query is kind of expensive, and it's used all over the place. Data is cached for 5 minutes.
        /// </summary>
        /// <param name="start">Unix timestamp starttime</param>
        /// <param name="end">Unix timestamp endtime</param>
        /// <param name="franchiseid">
        ///     FranchiseID -- this will not default to the session franchiseid, because this data is cached
        ///     on the server
        /// </param>
        /// <returns></returns>
        [OutputCache(Location = OutputCacheLocation.ServerAndClient, Duration = 600, VaryByParam = "start;end;franchiseid")]
        public ActionResult GetSchedule(double start, double end, Guid franchiseid)
        {
            DateTime startTime = Date.UnixTimestampToDateTime(start);
            DateTime endTime = Date.UnixTimestampToDateTime(end);

            var repo = new ScheduleRepository();
            List<Schedule> scheduled = repo.GetBetween(franchiseid, startTime, endTime).ToList();

            var surveyRepo = new QuoteSurveyRepository();
            IEnumerable<QuoteSurvey> surveys = surveyRepo.GetBetween(franchiseid, startTime, endTime);

            var noteRepo = new ScheduleNoteRepository();
            IEnumerable<ScheduleNote> notes = noteRepo.GetBetween(startTime, endTime);

            IEnumerable<CalendarItemModel> ret =
                scheduled.Select(
                    i =>
                        new CalendarItemModel()
                        {
                            title = "Quote " + i.Quote.Lookup,
                            start = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.StartTime)),
                            end = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.EndTime)),
                            cost = i.Quote.GetPricePerDay(),
                            allDay = false
                        });

            ret =
                ret.Concat(
                    surveys.Select(
                        i =>
                            new CalendarItemModel()
                            {
                                title = "In Home -- " + i.Quote.Lookup,
                                start = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.TimeStart.Hours)),
                                end = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.TimeEnd.Hours)),
                                cost = 0,
                                allDay = false
                            }));

            ret =
                ret.Concat(
                    notes.Where(i => !String.IsNullOrWhiteSpace(i.Message))
                        .Select(
                            i =>
                                new CalendarItemModel()
                                {
                                    title = "NOTE: " + i.Message,
                                    start = Date.DateTimeToUnixTimestamp(new DateTime(i.Year, i.Month, i.Day).ToLocalTime().AddHours(12)),
                                    end = Date.DateTimeToUnixTimestamp(new DateTime(i.Year, i.Month, i.Day).ToLocalTime().AddHours(12).AddMilliseconds(1)),
                                    cost = 0,
                                    allDay = true
                                }));

            return Json(ret, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddStorageAccount(string quoteid, string storageid, StorageQuoteType type)
        {
            var repo = new QuoteRepository();
            var storageRepo = new StorageWorkOrderRepository();

            Quote quote = repo.Get(quoteid);
            StorageWorkOrder storage = storageRepo.Get(storageid);

            if (quote == null || storage == null)
            {
                return Json(new ErrorModel("storageid", "Invalid storage account"));
            }

            var rel = new StorageWorkOrder_Quote_Rel { QuoteID = quote.QuoteID, StorageWorkOrderID = storage.WorkOrderID, StorageQuoteTypeID = (int)type };

            storage.StorageWorkOrder_Quote_Rel.Add(rel);
            storageRepo.Save();

            return Json("Success");
        }


        [HttpGet]
         [OutputCache(Location = OutputCacheLocation.ServerAndClient, Duration = 600, VaryByParam = "start;end;franchiseid")]
         public ActionResult GetSchedule1(string start, string end, Guid franchiseid)
        {
            DateTime startTime = Convert.ToDateTime(start);//Date.UnixTimestampToDateTime(start);
            DateTime endTime = Convert.ToDateTime(end);//Date.UnixTimestampToDateTime(end);

             var repo = new ScheduleRepository();
             List<Schedule> scheduled = repo.GetBetween(franchiseid, startTime, endTime).ToList();

             var surveyRepo = new QuoteSurveyRepository();
             IEnumerable<QuoteSurvey> surveys = surveyRepo.GetBetween(franchiseid, startTime, endTime);

             var noteRepo = new ScheduleNoteRepository();
             IEnumerable<ScheduleNote> notes = noteRepo.GetBetween(startTime, endTime);

             IEnumerable<CalendarItemModel> ret =
                 scheduled.Select(
                     i =>
                         new CalendarItemModel()
                         {
                             title = "Quote " + i.Quote.Lookup,
                             start = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.StartTime)),
                             end = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.EndTime)),
                             cost = i.Quote.GetPricePerDay(),
                             allDay = false
                         });

             ret =
                 ret.Concat(
                     surveys.Select(
                         i =>
                             new CalendarItemModel()
                             {
                                 title = "In Home -- " + i.Quote.Lookup,
                                 start = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.TimeStart.Hours)),
                                 end = Date.DateTimeToUnixTimestamp(i.Date.AddHours(i.TimeEnd.Hours)),
                                 cost = 0,
                                 allDay = false
                             }));

             ret =
                 ret.Concat(
                     notes.Where(i => !String.IsNullOrWhiteSpace(i.Message))
                         .Select(
                             i =>
                                 new CalendarItemModel()
                                 {
                                     title = "NOTE: " + i.Message,
                                     start = Date.DateTimeToUnixTimestamp(new DateTime(i.Year, i.Month, i.Day).ToLocalTime().AddHours(12)),
                                     end = Date.DateTimeToUnixTimestamp(new DateTime(i.Year, i.Month, i.Day).ToLocalTime().AddHours(12).AddMilliseconds(1)),
                                     cost = 0,
                                     allDay = true
                                 }));

             return Json(ret, JsonRequestBehavior.AllowGet);
         }

        /// <summary>
        ///     Add a quote comment, sent from quotes.js
        ///     POST /Quote/AddComment
        /// </summary>
        /// <param name="quoteid"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddComment(Guid quoteid, string text)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);
            if (quote == null)
            {
                return HttpNotFound();
            }

            if (VerifyRead(quote))
            {
                QuoteComment comment = quote.AddComment(AspUserID, text);
                quote.UpdateModifiedDate(AspUserID);
                repo.Save();
                return Json(comment.ToJsonObject());
            }

            return HttpNotFound();
        }

        // GET /Quote/GetComments
        [HttpPost]
        public ActionResult GetComments(Guid quoteid)
        {
            var repo = new QuoteRepository();
            Quote opp = repo.Get(quoteid);
            if (VerifyRead(opp))
            {
                return Json(opp.GetComments().Select(i => i.ToJsonObject()));
            }

            return HttpNotFound();
        }

        // Get/POST /Quote/GetQuicklook
        public ActionResult GetQuicklook(Guid quoteid)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);
            if (VerifyRead(quote))
            {
                return Json(quote.GetQuicklook(), JsonRequestBehavior.AllowGet);
            }

            return HttpNotFound();
        }
        public ActionResult GetQuicklookByLookup(string lookup)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(lookup);
            if (VerifyRead(quote))
            {
                return Json(quote.GetQuicklook(), JsonRequestBehavior.AllowGet);
            }

            return HttpNotFound();
        }
      
        // GET /Quote/Stops/{lookup}
        public ActionResult Stops(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                quote.OrderStops();
                repo.Save();
                var model = new QuoteStopsModel(quote);
                return View(model);
            }

            return HttpNotFound();
        }


        [HttpGet]

        public JsonResult StopsJson(string id)
        {

            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                //quote.OrderStops();
                //repo.Save();
                //var model = new QuoteStopsModel(quote);
                var stops = quote.Stops.Select(m=>m.ToJsonObject(true)).ToList();
                
                
                return Json(stops, JsonRequestBehavior.AllowGet);

            }
            return null;

        }


        [HttpGet]

        public JsonResult FranchiseDetails(string id)
        {
           
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                quote.OrderStops();
                repo.Save();
                var model = new QuoteStopsModel(quote);
                var stops = model.Quote.Stops.ToList();
                var address = model.Quote.GetFranchiseAddress();
                return Json(new
                {
                   
                    franchiseAddress = address.DisplayString() , 
                    franchiseID = address.AddressID



                },JsonRequestBehavior.AllowGet);
                
            }
            return null;

        }

        // GET /Quote/Inventory/{lookup}
        public ActionResult Inventory(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                var model = new QuoteInventoryModel(quote);
                return View(model);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public JsonResult InventoryItems(string lookup)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(lookup);
            if (VerifyRead(quote))
            {
                var model = new QuoteInventoryModel(quote);
                var qitems = model.GetItems().ToList();
                var qboxes = model.GetBoxes().ToList();
                var qcustomitems = model.GetCustomItems().ToList();
                var t = qitems.Select(m => m.ToJsonObject()).OrderBy(i => i.Name).SerializeToJson();
                var s = qboxes.Select(b => b.ToJsonObject()).SerializeToJson();
                var z = qcustomitems.Select(itm => itm.ToJsonObject()).OrderBy(i => i.Name).SerializeToJson();
                return Json(new
                {
                    items = t,
                    boxes = s,
                    customitems = z

                },JsonRequestBehavior.AllowGet);

            }
            return null;
        }




        // GET /Quote/PricingBreakdown/{lookup}
        public ActionResult PricingBreakdown(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuoteOverviewModel(quote));
            }

            return HttpNotFound();
        }

        // GET /Quote/Pricing/{lookup}
        public ActionResult Pricing(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuotePricingModel(quote));
            }

            return HttpNotFound();
        }


        [HttpGet]
        public JsonResult PricingJson(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                var qu = (new QuotePricingModel(quote).ToJsonObject());
                
                return Json(qu,JsonRequestBehavior.AllowGet);
            }

            return null;
        }

        [HttpPost]
        public void DiscountPriority(string id, string discountType,string percent)
      {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {

               if (discountType.Equals("value")) 
               {
                   quote.DiscountPriority = (int)(DiscountType.DiscountByValue);
                   quote.AdjustmentPercentage = decimal.Parse("0");
               }
               else if (discountType.Equals("percent"))
               {
                   quote.DiscountPriority = (int)(DiscountType.DiscountbyPercentage);
                   quote.AdjustmentPercentage = decimal.Parse(percent);
               }
               repo.Save();
               //repo.UpdateSavedItemList(quote);
            }
        }

        [HttpPost]
        public int GetDiscountPriority(string id) { 
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                if (quote.DiscountPriority==null)
                {
                    return -1;
                }
                else
                {
                    return (int)quote.DiscountPriority;
                }
                

            }
            return -1;
        }

        [HttpPost]
        public decimal? DiscountPercentage(string id) {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            
            
            if (VerifyRead(quote))
            {
                if (quote.DiscountPriority.Equals((int)DiscountType.DiscountbyPercentage))
                {
                    return quote.AdjustmentPercentage;                   
                }
                else
                {
                    return null;
                }
                

            }

            return null;

        }

        // GET /Quote/Overview/{lookup}
        public ActionResult Overview(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuoteOverviewModel(quote));
            }

            return HttpNotFound();
        }

        // GET /Quote/Overview/{lookup}
        public ActionResult InventoryHistory(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            VerifyRead(quote);
            return View(new QuoteOverviewModel(quote));
        }

        public ActionResult FieldStatus(string id)
        {
            var obj = new FieldStatusViewModel(id);
            return View(obj);
        }

        public ActionResult GetMapsData(string quoteId)
        {
            return null;
            //var repo = new QuoteRepository();
            //var quote = repo.Get(quoteId);

            //var data = new
            //{

            //}
        }

        





        // GET /Quote/Schedule/{lookup}
        public ActionResult Schedule(string id, int? month = null, bool force = false)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                if (!force && quote.GetSchedules().Any())
                {
                    Schedule schedule = quote.GetSchedules().First();
                    return RedirectToAction("ScheduleDay", new { id = id, day = schedule.Date.Day, month = schedule.Date.Month, year = schedule.Date.Year });
                }

                var employeeRepo = new EmployeeRepository();
                var vehicleRepo = new VehicleRepository();
                return
                    View(
                        new QuoteScheduleModel(
                            quote.FranchiseID,
                            quote,
                            month ?? DateTime.Now.Month,
                            employeeRepo.GetAllIncludingArchived(quote.FranchiseID),
                            vehicleRepo.GetAll(quote.FranchiseID)));
            }

            return HttpNotFound();
        }



        public ActionResult ScheduleJson(string id, int? month = null, bool force = false)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                if (!force && quote.GetSchedules().Any())
                {
                 //   Schedule schedule = quote.GetSchedules().First();
                 //   return RedirectToAction("ScheduleDay", new { id = id, day = schedule.Date.Day, month = schedule.Date.Month, year = schedule.Date.Year });
                }

                var employeeRepo = new EmployeeRepository();
                var vehicleRepo = new VehicleRepository();
                return
                    Json(
                        new QuoteScheduleModel(
                            quote.FranchiseID,
                            quote,
                            month ?? DateTime.Now.Month,
                            employeeRepo.GetAllIncludingArchived(quote.FranchiseID),
                            vehicleRepo.GetAll(quote.FranchiseID)).ToJsonObject(),JsonRequestBehavior.AllowGet);
            }
            return null;
            //return HttpNotFound();
        }




        // GET /Quote/ScheduleDay/{lookup}
        public ActionResult ScheduleDay(string id, int day, int month, int year)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                var scheduleRepo = new ScheduleRepository();
                var date = new DateTime(year, month, day);
                IEnumerable<Schedule> scheduled = scheduleRepo.GetForDay(quote.FranchiseID, date);

                var employeeRepo = new EmployeeRepository();
                var vehicleRepo = new VehicleRepository();

                var surveyRepo = new QuoteSurveyRepository();
                IEnumerable<QuoteSurvey> survey = surveyRepo.GetForDay(quote.FranchiseID, new DateTime(year, month, day));

                return
                    View(
                        new QuoteScheduleModel(
                            quote.FranchiseID,
                            quote,
                            date,
                            scheduled,
                            employeeRepo.GetAllIncludingArchived(quote.FranchiseID),
                            vehicleRepo.GetAll(quote.FranchiseID),
                            survey));
            }

            return HttpNotFound();
        }
        [HttpGet]
        public JsonResult ScheduleDayJson(string id, int day, int month, int year)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                var scheduleRepo = new ScheduleRepository();
                var date = new DateTime(year, month, day);
                IEnumerable<Schedule> scheduled = scheduleRepo.GetForDay(quote.FranchiseID, date);

                var employeeRepo = new EmployeeRepository();
                var vehicleRepo = new VehicleRepository();

                var surveyRepo = new QuoteSurveyRepository();
                IEnumerable<QuoteSurvey> survey = surveyRepo.GetForDay(quote.FranchiseID, new DateTime(year, month, day));

                return
                    Json(
                        new QuoteScheduleModel(
                            quote.FranchiseID,
                            quote,
                            date,
                            scheduled,
                            employeeRepo.GetAllIncludingArchived(quote.FranchiseID),
                            vehicleRepo.GetAll(quote.FranchiseID),
                            survey).ToJsonObject(),JsonRequestBehavior.AllowGet
                            );
            }
            return null;
            //return HttpNotFound();
        }













        // GET /Quote/MoveUp/{stopid}
        public ActionResult MoveUp(Guid stopid)
        {
            var repo = new StopRepository();
            Stop stop = repo.Get(stopid);

            var quoterepo = new QuoteRepository();
            Quote quote = quoterepo.Get(stop.QuoteID);

            if (VerifyWrite(quote))
            {
                List<Stop> stops = quote.GetStops().ToList();

                // NOTE: Due to the way the UI is setup, it isn't possible for either of these to be non-existent
                Stop oldStop = stops.First(i => i.Sort == stop.Sort - 1);
                Stop newStop = stops.First(i => i.StopID == stop.StopID);

                oldStop.Sort = stop.Sort;
                newStop.Sort = stop.Sort - 1;

                quote.UpdateModifiedDate(AspUserID);
                quote.AddLog(AspUserID, "Stop order changed");
                quoterepo.UpdateGuaranteedPrice(quote.QuoteID);
                quoterepo.Save();

                return RedirectToAction("Stops", new { id = quote.Lookup });
            }

            return HttpNotFound();
        }

        /// <summary>
        ///     Delete a stop -- sent from StopModal
        ///     GET /Quote/DeleteStop/{stopid}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteStop(Guid id)
        {
            var quoterepo = new QuoteRepository();
            var repo = new StopRepository();
            Stop stop = repo.Get(id);
            Quote quote = quoterepo.Get(stop.QuoteID);

            if (VerifyWrite(quote))
            {
                repo.Remove(stop);
                repo.Save();

                quote.OrderStops();

                quote.UpdateModifiedDate(AspUserID);
                quote.AddLog(AspUserID, "Stop removed");
                quoterepo.UpdateGuaranteedPrice(quote.QuoteID);
                quoterepo.Save();

                return RedirectToAction("Stops", new { id = quote.Lookup });
            }

            return HttpNotFound();
        }

        [HttpGet]
        public JsonResult DeleteStopJSON(string idd)
        {
            var id = new Guid(idd);
            var quoterepo = new QuoteRepository();
            var repo = new StopRepository();
            Stop stop = repo.Get(id);
            Quote quote = quoterepo.Get(stop.QuoteID);

            if (VerifyWrite(quote))
            {
                repo.Remove(stop);
                repo.Save();

                quote.OrderStops();

                quote.UpdateModifiedDate(AspUserID);
                quote.AddLog(AspUserID, "Stop removed");
                quoterepo.UpdateGuaranteedPrice(quote.QuoteID);
                quoterepo.Save();

                return Json("OK", JsonRequestBehavior.AllowGet);
            }

            return Json("ERROR", JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        ///     Save inventory for stops, sent from inventory.js
        ///     POST /Quote/Inventory/
        /// </summary>
        /// <param name="quoteid"></param>
        /// <param name="rooms"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Inventory(Guid quoteid, string rooms)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);
            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            var addedRooms = new Dictionary<string, Guid>();
            var roomrepo = new RoomRepository();

            // if the quote has already been printed, we'll flag the items we add
            bool printed = quote.Postings.Any();

            List<RoomJson> allRoomsJson = General.DeserializeJson<IEnumerable<RoomJson>>(rooms).ToList();
            IEnumerable<Guid> jsonRoomIds = allRoomsJson.Where(i => General.IsGuid(i.RoomID)).Select(i => Guid.Parse(i.RoomID));
            List<Room> currentRooms = quote.GetStops().SelectMany(i => i.GetRooms()).ToList();
            IEnumerable<Room> toRemove = currentRooms.Where(i => !jsonRoomIds.Contains(i.RoomID));

            foreach (RoomJson roomJson in allRoomsJson)
            {
                Guid roomid;
                if (Guid.TryParse(roomJson.RoomID, out roomid))
                {
                    Room room = roomrepo.Get(roomid);
                    room.Sort = roomJson.Sort;

                    IEnumerable<Room_InventoryItem> changes = room.SetItems(roomJson.Items);
                    foreach (Room_InventoryItem rel in changes)
                    {
                        rel.AddedAfter = printed;
                    }

                    room.StopID = roomJson.StopID;
                    room.Name = roomJson.Type;
                    room.Description = roomJson.Description;
                    room.Pack = roomJson.Pack;
                    roomrepo.Save();
                }
                else
                {
                    var room = new Room(roomJson);
                    foreach (Room_InventoryItem rel in room.Room_InventoryItems)
                    {
                        rel.AddedAfter = printed;
                    }

                    // ID passed up from page
                    string oldID = roomJson.RoomID;
                    if (string.IsNullOrEmpty(oldID))
                    {
                        oldID = Convert.ToString(Guid.NewGuid());
                    }

                    roomrepo.Add(room);
                    roomrepo.Save();

                    addedRooms.Add(oldID, room.RoomID);
                }
            }

            foreach (Room room in toRemove)
            {
                roomrepo.Remove(roomrepo.Get(room.RoomID));
                roomrepo.Save();
            }

            quote.UpdateModifiedDate(AspUserID);
            repo.UpdateGuaranteedPrice(quote.QuoteID);
            repo.Save();

            return Json(addedRooms);
        }

        /// <summary>
        ///     Save guaranteed move parameters, sent from pricing.js
        ///     POST /Quote/SaveGuaranteed
        /// </summary>
        [HttpPost]
        public ActionResult SaveGuaranteed(
            Guid quoteid,
            decimal adjustment,
            int trucks,
            decimal? packingMaterials,
            Guid valuationType,
            string discountCouponCode,
            int? forcedStorage = null)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);

            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            decimal basePrice = quote.CalculateGuaranteedPrice();
            if (basePrice == 0 || Math.Floor((adjustment * 100) / basePrice) > QuoteRepository.MaxPriceDiscount)
            {
                adjustment = 0;
            }

            quote.AddService(ServiceType.PackingMaterials, packingMaterials ?? 0m);
            quote.AddService(ServiceType.StorageFees, quote.GetTemporaryStorageCost());
            quote.ForcedStorageCost = forcedStorage;

            var valuationRepo = new ReplacementValuationRepository();
            ReplacementValuation valuation = valuationRepo.Get(valuationType);
            quote.ValuationTypeID = valuation.ValuationTypeID;
            quote.ReplacementValuationCost = quote.GetReplacementValuationCost(valuation.ValuationTypeID, QuotePricingType.Binding);

            if (!string.IsNullOrEmpty(discountCouponCode) && quote.DiscountCouponId == null)
            {
                var disco = UseDiscountCouponCoupon(discountCouponCode);
                quote.DiscountCouponId = disco.DiscountCouponId;
                quote.DiscountCouponCode = disco.CouponCode;
                quote.DiscountCopounUsed = true;
                repo.Save();
            }

            quote.Trucks = trucks;
            quote.CrewSize = 2 * trucks;

            quote.UpdateModifiedDate(AspUserID);
            repo.UpdateGuaranteedPrice(quote.QuoteID, adjustment, true);
            repo.Save();
            return Json("Success");
        }

        /// <summary>
        ///     Saves hourly move parameters, sent from pricing.js
        ///     POST /Quote/SaveHourly
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveHourly(
            Guid quoteid,
            int? numTrucks,
            int? crewSize,
            int estimateTime,
            decimal? packingMaterials,
            decimal? additionalDestination,
            Guid valuationType,
            int? forcedStorage = null)
        {
            // sometimes these dropdowns don't get populated on post -- default to 2 men 1 truck
            crewSize = crewSize ?? 2;
            numTrucks = numTrucks ?? 1;

            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);

            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            decimal perTruck = quote.GetHourlyTruckMultiplier();
            decimal perPerson = quote.GetHourlyPersonMultiplier();
            decimal perPersonDestination = quote.GetHourlyPersonDestinationMultiplier();
            decimal perTruckMultiplierDestination = quote.GetHourlyTruckDestinationMultiplier();

            quote.AddService(ServiceType.PackingMaterials, packingMaterials ?? 0m);

            var valuationRepo = new ReplacementValuationRepository();
            ReplacementValuation valuation = valuationRepo.Get(valuationType);
            quote.ValuationTypeID = valuation.ValuationTypeID;

            quote.ForcedStorageCost = forcedStorage;

            decimal perHour = (perPerson * crewSize.Value) + (perTruck * numTrucks.Value);
            decimal firstHour = (perPersonDestination * crewSize.Value) + (numTrucks.Value * perTruckMultiplierDestination) + perHour;

            if (additionalDestination.HasValue)
            {
                firstHour += additionalDestination.Value;
            }

            var hourlyData = new HourlyInfo() { CustomerTimeEstimate = estimateTime, FirstHourPrice = firstHour, HourlyPrice = perHour };

            quote.HourlyData = hourlyData;
            quote.Trucks = numTrucks.Value;
            quote.CrewSize = crewSize.Value;
            quote.ReplacementValuationCost = quote.GetReplacementValuationCost(valuation.ValuationTypeID, QuotePricingType.Hourly);
            quote.UpdateModifiedDate(AspUserID);
            quote.AddLog(AspUserID, "Hourly data changed");
            quote.AddService(ServiceType.StorageFees, quote.GetTemporaryStorageCost());

            repo.Save();
            return Json(String.Empty);
        }

        [HttpGet]
        public ActionResult GetHourlyValuations(Guid quoteid, int? numTrucks, int? crewSize, int estimateTime)
        {
            crewSize = crewSize ?? 2;
            numTrucks = numTrucks ?? 1;

            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);

            decimal perTruck = quote.GetHourlyTruckMultiplier();
            decimal perPerson = quote.GetHourlyPersonMultiplier();
            decimal perPersonDestination = quote.GetHourlyPersonDestinationMultiplier();
            decimal perTruckMultiplierDestination = quote.GetHourlyTruckDestinationMultiplier();

            decimal perHour = (perPerson * crewSize.Value) + (perTruck * numTrucks.Value);
            decimal firstHour = (perPersonDestination * crewSize.Value) + (numTrucks.Value * perTruckMultiplierDestination) + perHour;

            var hourlyData = new HourlyInfo() { CustomerTimeEstimate = estimateTime, FirstHourPrice = firstHour, HourlyPrice = perHour };

            decimal moveprice = hourlyData.FirstHourPrice + ((hourlyData.CustomerTimeEstimate - 1) * hourlyData.HourlyPrice);

            IEnumerable<ReplacementValuation> replacementValuationOptionsHourly = GetReplacementValuationsForHourly(moveprice);

            ViewBag.IsAllowEdit = quote.CanUserEdit(User.Identity.Name);
            ViewBag.SelectValuation = quote.ValuationTypeID;

            return PartialView("_HourlyValuation", replacementValuationOptionsHourly);
        }

        public IEnumerable<ReplacementValuation> GetReplacementValuationsForHourly(decimal moveprice)
        {
            var replacementValuationRepo = new ReplacementValuationRepository();
            List<ReplacementValuation> replacementValuationOptionsHourlylocal = replacementValuationRepo.GetAll().ToList();
            replacementValuationOptionsHourlylocal.RemoveAll(type => type.Type == 3);
            replacementValuationOptionsHourlylocal.Single(model => model.Type == 2).Cost = (moveprice * 5m) / 100m;
            replacementValuationOptionsHourlylocal.Single(model => model.Type == 4).Cost = (moveprice * 15m) / 100m;
            return replacementValuationOptionsHourlylocal.OrderBy(model => model.Type);
        }

        /// <summary>
        ///     Updates all stops for a quote, sent from stops.js
        ///     POST /Quote/Stops/
        /// </summary>
        /// <param name="quoteid"></param>
        /// <param name="stopsjson"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Stops(Guid quoteid, string stopsjson)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);

            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            var stopRepo = new StopRepository();
            int sort = 0;

            var ret = new Dictionary<string, Guid[]>();
            foreach (StopJson stopjson in General.DeserializeJson<IEnumerable<StopJson>>(stopsjson).OrderBy(i => i.sort))
            {
                Guid stopid;
                if (Guid.TryParse(stopjson.id, out stopid))
                {
                    // EDIT STOP
                    Stop stop = stopRepo.Get(stopid);
                    stop.CopyJsonProperties(stopjson);
                    stop.Sort = sort;

                    if (stop.AddressType == StopAddressType.MooversStorage && sort == 0)
                    {
                        stop.StorageDays = -1;
                    }

                    stopRepo.Save();
                    var ids = new[] { stop.StopID, stop.AddressID };
                    if (ret.ContainsKey(stop.StopID.ToString()))
                    {
                        ret.Remove(stop.StopID.ToString());
                    }
                    ret.Add(stop.StopID.ToString(), ids);
                }
                else
                {
                    Guid tp = Guid.NewGuid(); 
                    string oldid = stopjson.id;
                    Stop stop = null;
                    if (!string.IsNullOrEmpty(oldid))
                    {
                        stop = new Stop(stopjson) { QuoteID = quoteid, Sort = sort };
                        stopRepo.Add(stop);
                        stopRepo.Save();

                        var ids = new[] { stop.StopID, stop.AddressID };
                        ret.Add(oldid, ids);
                    }
                    else
                    {
                        stop = new Stop(stopjson) { QuoteID = quoteid, Sort = sort , StopID = tp};
                        oldid = Convert.ToString(tp);
                        stopRepo.Add(stop);
                        stopRepo.Save();

                        var ids = new[] { stop.StopID, stop.AddressID };
                        ret.Add(oldid, ids);
                    }
                    

                    

                   
                }

                sort++;
            }

            decimal travelTime = quote.GetDriveTime();
            decimal sourceTime = travelTime - quote.GetDriveTime(false);
            bool force = sourceTime > QuoteRepository.MaxHourlySourceTime || travelTime > QuoteRepository.MaxHourlyTravelTime;

            quote.UpdateModifiedDate(AspUserID);
            quote.AddLog(AspUserID, "Stops changed");
            repo.UpdateGuaranteedPrice(quote.QuoteID, null, force);
            repo.Save();

            return Json(ret);
        }

        // GET /Quote/AddCompetitor
        public ActionResult AddCompetitor(Guid quoteid, Guid competitorID, string name)
        {
            var repo = new QuoteRepository();
            var competitorRepo = new CompetitorRepository();
            Competitor competitor = competitorRepo.Get(competitorID);
            Quote quote = repo.Get(quoteid);

            if (VerifyRead(quote))
            {
                Quote_Competitor_Rel rel = quote.AddCompetitor(competitor, name);
                quote.AddLog(AspUserID, "Competitor added");
                repo.Save();
                return Json(rel.RelID);
            }

            return HttpNotFound();
        }

        // GET /Quote/RemoveCompetitor
        public ActionResult RemoveCompetitor(Guid relid)
        {
            var relrepo = new Quote_Competitor_Rel_Repository();
            Quote_Competitor_Rel rel = relrepo.Get(relid);
            relrepo.Remove(rel);
            relrepo.Save();
            return Json(String.Empty);
        }

        /*** Scheduling Functions **/

        /// <summary>
        ///     Adds a date/truck schedule for quotes that already have payment information associated with them.
        ///     POST /Quote/ScheduleJob
        /// </summary>
        /// <param name="quoteid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ScheduleJobNoPayment(Guid quoteid, ScheduleModal model)
        {
            var quoteRepo = new QuoteRepository();
            Quote quote = quoteRepo.Get(quoteid);

            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            if (!model.day.HasValue)
            {
                return RedirectToAction(
                    "ScheduleDay",
                    new { id = quote.Lookup, day = DateTime.Today.Day, month = DateTime.Today.Month, year = DateTime.Today.Year });
            }

            quote.AddLog(AspUserID, "Truck added");

            var crewRepo = new CrewRepository();
            Crew crew = crewRepo.GetForDayLookup(model.day.Value, model.crew, SessionFranchiseID);
            if (model.crew > 0 && crew == null)
            {
                crew = new Crew(model.day.Value, model.crew, SessionFranchiseID);
                crewRepo.Add(crew);
                crewRepo.Save();
            }

            Schedule schedule = quote.GetScheduleForDay(model.day.Value);
            if (schedule != null)
            {
                if (model.crew > 0)
                {
                    schedule.AddCrew(model.crew, SessionFranchiseID);
                }
                else
                {
                    var repo = new ScheduleRepository();
                    repo.ClearCrews(schedule);
                    repo.Save();
                }

                quoteRepo.Save();
                return RedirectToAction(
                    "ScheduleDay",
                    new { id = quote.Lookup, day = model.day.Value.Day, month = model.day.Value.Month, year = model.day.Value.Year });
            }

            var newSchedule = new Schedule();
            model.UpdateSchedule(ref newSchedule);
            quote.SetStatus(AspUserID, QuoteStatus.Scheduled, "Scheduled for " + newSchedule.Date.ToShortDateString());
            quote.DateScheduled = DateTime.Now;

            var scheduleRepo = new ScheduleRepository();
            scheduleRepo.Add(newSchedule);
            scheduleRepo.Save();
            quoteRepo.Save();

            return Json("success");
        }

        /// <summary>
        ///     Adds a date/truck schedule
        ///     POST /Quote/ScheduleJob
        ///     TODO: This function should be refactored, probably put with PaymentController/AddPayment
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ScheduleJob(Guid quoteid, ScheduleModal model)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);

            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            if (quote.GetSchedules().Any(s => s.Date.Date == model.day.Value.Date && s.ScheduledOnCrew(model.crew)))
            {
                return Json("Already scheduled");
            }

            if (model.cardnumber != null)
            {
                model.cardnumber = model.cardnumber.Replace("-", String.Empty);
            }

            var validator = new ScheduleModalValidator();
            ValidationResult errors = validator.Validate(model);
            if (!errors.IsValid)
            {
                return Json(new ErrorModel(errors));
            }

            if (model.paymentType == "NEW_CARD" && !String.IsNullOrEmpty(model.cardnumber))
            {
                try
                {
                    var cardPayment = new CreditCardPayment();
                    TransactionResult token = cardPayment.GetCreditCardToken(
                        quote.Franchise,
                        model.name,
                        model.cardnumber,
                        model.expirationmonth,
                        model.expirationyear,
                        model.cvv2,
                        model.billingzip,
                        quote.Account.Lookup);
                    if (!token.Transaction_Approved)
                    {
                        var error = new ErrorModel("cardnumber", "Invalid credit card number");
                        return Json(error);
                    }

                    Account_CreditCard rel = quote.Account.AddCreditCard(quote.FranchiseID, token);
                    quote.AddCard(rel);
                    repo.Save();
                }
                catch (PaymentException e)
                {
                    return Json(new ErrorModel("cardnumber", e.Message));
                }
            }
            else if (model.paymentType == "NO_CARD")
            {
                repo.Save();
            }
            else if (!String.IsNullOrEmpty(model.paymentType))
            {
                Guid cardid = Guid.Parse(model.paymentType);
                quote.AddCard(cardid);
                repo.Save();
            }
            else
            {
                return Json(new ErrorModel("paymentType", "Please select a card to add on file"));
            }

            var scheduleRepo = new ScheduleRepository();
            var schedule = new Schedule();
            model.UpdateSchedule(ref schedule);
            scheduleRepo.Add(schedule);

            quote.SetStatus(AspUserID, QuoteStatus.Scheduled, "Scheduled for " + schedule.Date.ToShortDateString());
            quote.DateScheduled = DateTime.Now;
            scheduleRepo.Save();
            repo.Save();
            return Json("success");
        }

        /// <summary>
        ///     Updates a specific date/truck schedule for a quote
        /// </summary>
        /// <param name="scheduleid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UpdateSchedule(Guid scheduleid, ScheduleModal model)
        {
            var repo = new ScheduleRepository();
            Schedule schedule = repo.Get(scheduleid);
            if (VerifyWrite(schedule.Quote))
            {
                model.UpdateSchedule(ref schedule);
                schedule.Quote.AddLog(AspUserID, "Schedule changed");
                repo.Save();

                DateTime day = schedule.Date;
                return RedirectToAction(
                    "ScheduleDay",
                    new { Controller = "Quote", id = schedule.Quote.Lookup, day = day.Day, month = day.Month, year = day.Year });
            }

            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult UpdateCard(Guid quoteid, AddCardModel model)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);
            string custID = quote.Account.Lookup;

            if (!VerifyWrite(quote))
            {
                return HttpNotFound();
            }

            if (model.cardnumber != null)
            {
                model.cardnumber = model.cardnumber.Replace("-", String.Empty);
            }

            try
            {
                var cardPayment = new CreditCardPayment();
                TransactionResult token = cardPayment.GetCreditCardToken(
                    quote.Franchise,
                    model.name,
                    model.cardnumber,
                    model.expirationmonth,
                    model.expirationyear,
                    model.cvv2,
                    model.billingzip,
                    custID);
                if (!token.Transaction_Approved)
                {
                    var error = new ErrorModel("cardnumber", "Invalid credit card number");
                    return Json(error);
                }

                Account_CreditCard rel = quote.Account.AddCreditCard(quote.Franchise.FranchiseID, token);
                quote.UpdateModifiedDate(AspUserID);
                quote.AddLog(AspUserID, "Card on file changed");
                quote.AddCard(rel);
                repo.Save();
            }
            catch (PaymentException e)
            {
                return Json(new ErrorModel("cardnumber", e.Message));
            }

            return Json("Success");
        }

        /// <summary>
        ///     Cancels a specific date/truck schedule for a quote.
        ///     GET /Quote/Reschedule
        /// </summary>
        /// <param name="scheduleid"></param>
        /// <param name="redirect"></param>
        /// <returns></returns>
        public ActionResult Reschedule(Guid scheduleid, string redirect)
        {
            var repo = new ScheduleRepository();
            Schedule schedule = repo.Get(scheduleid);

            if (!VerifyWrite(schedule.Quote))
            {
                return HttpNotFound();
            }

            schedule.Cancel(AspUserID);

            repo.Save();
            Quote quote = schedule.Quote;
            if (!quote.GetSchedules().Any())
            {
                quote.SetStatus(AspUserID, QuoteStatus.Open, "Rescheduling quote");
                repo.Save();
            }

            return Redirect(redirect);
        }

        /// <summary>
        ///     Changes the estimated move date for a quote. This does not effect any of the actual scheduling.
        ///     GET /Quote/ChangeMoveDate
        /// </summary>
        /// <param name="quoteid"></param>
        /// <param name="movedate"></param>
        /// <param name="redirect"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeMoveDate(Guid quoteid, DateTime movedate, string redirect)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(quoteid);
            if (VerifyWrite(quote))
            {
                quote.UpdateModifiedDate(AspUserID);
                quote.AddLog(AspUserID, String.Format("Move Date changed from {0} to {1}", quote.MoveDate.ToShortDateString(), movedate.ToShortDateString()));
                quote.MoveDate = movedate;
                repo.Save();

                return Redirect(redirect);
            }

            return HttpNotFound();
        }

        /// <summary>
        ///     Sets notes for a move, used on schedules
        ///     POST /Quote/AddNote
        /// </summary>
        /// <param name="scheduleid"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddNote(Guid scheduleid, string note)
        {
            var repo = new ScheduleRepository();
            Schedule schedule = repo.Get(scheduleid);

            if (!VerifyRead(schedule.Quote))
            {
                return HttpNotFound();
            }

            schedule.Note = note;
            schedule.Quote.AddLog(AspUserID, "Schedule note added");
            schedule.Quote.UpdateModifiedDate(AspUserID);
            repo.Save();

            return Json(schedule.ToJsonObject());
        }

        [HttpGet]
        public ActionResult ScheduleSurvey(string id, DateTime? date)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            if (!VerifyRead(quote))
            {
                return HttpNotFound();
            }

            if (!date.HasValue && quote.GetSurveys().Any())
            {
                date = quote.QuoteSurveys.Select(i => i.Date).Min();
            }
            else
            {
                date = date ?? DateTime.Today;
            }

            quote.AddLog(AspUserID, "Visual Survey (" + date.Value.ToShortDateString() + ") scheduled");

            var surveyRepo = new QuoteSurveyRepository();
            IEnumerable<QuoteSurvey> surveys = surveyRepo.GetForDay(quote.FranchiseID, date.Value);

            var model = new QuoteSurveyModel(quote.FranchiseID, quote, surveys, date.Value);
            return View(model);
        }

        [HttpPost]
        public ActionResult CancelSurvey(Guid id, string redirectid)
        {
            var surveyRepo = new QuoteSurveyRepository();
            QuoteSurvey survey = surveyRepo.Get(id);
            if (!VerifyRead(survey.Quote))
            {
                return HttpNotFound();
            }

            survey.Quote.UpdateModifiedDate(AspUserID);
            survey.Quote.AddLog(AspUserID, String.Format("Visual Survey ({0}) cancelled", survey.Date.ToShortDateString()));
            survey.IsCancelled = true;
            surveyRepo.Save();
            return RedirectToAction("ScheduleSurvey", new { id = redirectid, date = survey.Date });
        }

        [HttpPost]
        public ActionResult ScheduleSurvey(Guid id, DateTime date, int timestart, int timeend)
        {
            var repo = new QuoteSurveyRepository();
            var quoteRepo = new QuoteRepository();
            Quote quote = quoteRepo.Get(id);

            if (!VerifyRead(quote))
            {
                return HttpNotFound();
            }

            quote.UpdateModifiedDate(AspUserID);
            quoteRepo.Save();

            var survey = new QuoteSurvey(quote.QuoteID, date, new TimeSpan(timestart, 0, 0), new TimeSpan(timeend, 0, 0));
            repo.Add(survey);
            repo.Save();

            return RedirectToAction("ScheduleSurvey", new { id = quote.Lookup, date = date });
        }

        [HttpPost]
        public ActionResult SetSurveyNotes(Guid surveyid, string note)
        {
            var repo = new QuoteSurveyRepository();
            QuoteSurvey survey = repo.Get(surveyid);
            survey.Notes = note;
            repo.Save();
            return Json("success");
        }

        /*** Overview Functions **/

        public ActionResult SavePrintedComments(string id, string comments)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyWrite(quote))
            {
                quote.PrintedComments = comments;
                quote.UpdateModifiedDate(AspUserID);
                repo.Save();

                return RedirectToAction("Overview", new { Controller = "Quote", id = id });
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult AccessLog(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuoteEdit(quote, "Overview"));
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult EmailLog(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuoteEdit(quote, "Overview"));
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Files(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuoteEdit(quote, "Overview"));
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult SendEmail(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyRead(quote))
            {
                return View(new QuoteSendEmailModel(quote, AspUser.aspnet_Membership.Email));
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Posting(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            var employeeRepo = new EmployeeRepository();
            var vehicleRepo = new VehicleRepository();

            ViewBag.Employees = employeeRepo.GetAll(quote.FranchiseID);
            ViewBag.Vehicles = vehicleRepo.GetAll(quote.FranchiseID);

            if (!quote.Schedules.Any(i => i.Postings.Any()))
            {
                return RedirectToAction("Overview", new { id = id });
            }

            if (VerifyRead(quote))
            {
                return View(new QuoteOverviewModel(quote));
            }

            return HttpNotFound();
        }

        [HttpPost]
        public ActionResult SendEmail(
            string id,
            string to,
            string from,
            string subject,
            string message,
            string cc = "",
            string bcc = "",
            IEnumerable<string> fileids = null)
        {
            IEnumerable<string> ccs = cc.Split(',').Select(i => i.Trim()).Where(i => !String.IsNullOrWhiteSpace(i));
            IEnumerable<string> bccs = bcc.Split(',').Select(i => i.Trim()).Where(i => !String.IsNullOrWhiteSpace(i));

            var quoteRepo = new QuoteRepository();
            Quote quote = quoteRepo.Get(id);

            if (!VerifyRead(quote))
            {
                return HttpNotFound();
            }

            quote.UpdateModifiedDate(AspUserID);
            quoteRepo.Save();

            aspnet_Users_Profile accountManager = quote.AccountManager.aspnet_Users_Profile.First();
            var aspUserRepo = new aspnet_UserRepository();
            aspnet_User aspUser = aspUserRepo.GetByEmail(from);

            if (aspUser != null && aspUser.aspnet_Users_Profile.Any())
            {
                accountManager = aspUser.aspnet_Users_Profile.First();
            }

            string html = RenderViewToString(
                "Emails/GenericEmail",
                new EmailModel()
                {
                    To = to,
                    From = from,
                    Account = quote.Account,
                    Message = Email.PlainTextToHtml(message),
                    AccountManager = accountManager,
                    Franchise = quote.Franchise
                });

            if (String.IsNullOrEmpty(to))
            {
                ModelState.AddModelError("to", "To Required");
            }

            if (String.IsNullOrEmpty(from))
            {
                ModelState.AddModelError("from", "From Required");
            }

            if (String.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("message", "Message required");
            }

            if (String.IsNullOrEmpty(subject))
            {
                ModelState.AddModelError("subject", "Subject required");
            }

            if (!ModelState.IsValid)
            {
                return View(new QuoteSendEmailModel(quote, aspUser.aspnet_Membership.Email));
            }

            fileids = fileids ?? Enumerable.Empty<string>();
            IList<File> files = fileids.Where(x => !x.Equals("CustomerResponsibilityChecklist") && !x.Equals("YourRightsandProtectionofYourProperty")).Select(i => GetQuoteFile(quote.QuoteID, i)).ToList();
            if (fileids.Any(x=>x.Equals("CustomerResponsibilityChecklist")))
            {
                files.Add(new File("CustomerResponsibilityChecklist.pdf", "application/pdf"));
            }

            if (fileids.Any(x => x.Equals("YourRightsandProtectionofYourProperty")))
            {
                files.Add(new File("YourRightsandProtectionofYourProperty.pdf", "application/pdf"));
            }

            Email.SendLoggedEmail(quoteRepo, quote, to, from, ccs, bccs, subject, html, files, EmailCategory.Proposal);
            return RedirectToAction("EmailLog", new { id = id });
        }

        [HttpPost]
        public ActionResult ChangeUser(string id, Guid userid, string redirect, Guid? franchiseid = null)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (VerifyWrite(quote))
            {
                quote.AccountManagerID = userid;

                if (franchiseid.HasValue)
                {
                    quote.FranchiseID = franchiseid.Value;
                    quote.ForceFranchiseID = franchiseid.Value;
                }

                repo.Save();

                return Redirect(redirect);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewFile(string id, Guid fileid)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            if (quote.CanUserRead(User.Identity.Name))
            {
                File file = GetQuoteFile(quote.QuoteID, fileid.ToString());
                return File(file.SavedContent, "application/pdf");
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewContract(string id)
        {
            ////// To render content as HTML
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (quote.CanUserRead(User.Identity.Name))
            {
                ////var html = RenderViewToString("PDFS/_Contract", new Business.ViewModels.ContractPrintModel(quote, quote.Schedules.First()));
                ////return Content(html, "text/html");
                File file = GetQuoteFile(quote.QuoteID, "-New Contract-");
                return File(file.SavedContent, file.ContentType);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewProposal(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (quote.CanUserRead(User.Identity.Name))
            {
                //////To render as html
                ////var html = RenderViewToString("PDFS/_Proposal", quote);
                ////return Content(html, "text/html");

                File file = GetQuoteFile(quote.QuoteID, "-New Proposal-");
                return File(file.SavedContent, file.ContentType);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewReceipt(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (quote.CanUserRead(User.Identity.Name))
            {
                //////To render as html
                ////var html = RenderViewToString("PDFS/_Receipt", quote);
                ////return Content(html, "text/html");
                File file = GetQuoteFile(quote.QuoteID, "-New Receipt-");
                return File(file.SavedContent, file.ContentType);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewStorageAccess(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (quote.CanUserRead(User.Identity.Name))
            {
                //////To render as html
                ////var html = RenderViewToString("PDFS/_StorageAccess", quote);
                ////return Content(html, "text/html");
                File file = GetQuoteFile(quote.QuoteID, "-Storage Access-");
                return File(file.SavedContent, file.ContentType);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewSpecialInvoice(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            if (quote.CanUserRead(User.Identity.Name))
            {
                //////To render as html
                ////var html = RenderViewToString("PDFS/_SpecialInvoice", quote);
                ////return Content(html, "text/html");
                File file = GetQuoteFile(quote.QuoteID, "-New SpecialInvoice-");
                return File(file.SavedContent, file.ContentType);
            }

            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult ViewInvoice(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            if (quote.CanUserRead(User.Identity.Name))
            {
                //////To render as html
                ////var html = RenderViewToString("PDFS/_Invoice", quote);
                ////return Content(html, "text/html");
                string html = RenderViewToString("PDFS/_Invoice", quote);
                return Content(html, "text/html");
            }

            return HttpNotFound();
        }

        public ActionResult SchedulePackJob(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            Quote packer = quote.Duplicate(AspUserID);
            packer.IsPackJob = true;
            repo.Add(packer);
            repo.Save();

            return RedirectToAction("Overview", new { id = repo.Get(packer.QuoteID).Lookup });
        }

        public ActionResult ReopenForIPad(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);
            if (quote != null)
            {
                quote.Stops.ToList().ForEach(s => s.QuoteStatus.DeleteAll());
            }
            repo.Save();
            return View("Overview", new QuoteOverviewModel(quote));
        }

        [HttpGet]
        public ActionResult ValidateCoupon(string code)
        {
            var discorepo = new DiscountCouponRepository();
            DiscountCoupon discountcoupon = discorepo.GetByCouponCode(code);
            if (discountcoupon != null && discountcoupon.IsPublished && !discountcoupon.IsExpired
                && (discountcoupon.DurationEnd >= DateTime.Now || discountcoupon.DurationEnd == null)
                && discountcoupon.NumberOfTimesUsable > discountcoupon.NumberOfTimesUsed)
            {
                var data =
                    new
                    {
                        isValid = true,
                        value = discountcoupon.Percentage > 0.00m ? discountcoupon.Percentage : discountcoupon.Amount,
                        valueType = discountcoupon.Percentage > 0.00m ? 1 : 2
                    };
                return Json(data, JsonRequestBehavior.AllowGet);
            }

            var errordata = new { isValid = false };

            return Json(errordata, JsonRequestBehavior.AllowGet);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // any time we access a route with an "ID" parameter via "GET", add an access log
            if (filterContext.HttpContext.Request.HttpMethod == "GET" && filterContext.ActionParameters.Any(i => i.Key == "id" && i.Value is string))
            {
                var val = (string)filterContext.ActionParameters.First(i => i.Key == "id").Value;
                var action = (string)filterContext.RouteData.Values["action"];
                var repo = new QuoteRepository();
                Quote quote = repo.Get(val);
                if (quote != null)
                {
                    var logRepo = new QuoteAccessLogRepository();
                    logRepo.Log(action, quote.QuoteID, AspUserID);
                    logRepo.Save();
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private DiscountCoupon UseDiscountCouponCoupon(string couponCode)
        {
            var discorepo = new DiscountCouponRepository();
            DiscountCoupon discountcoupon = discorepo.GetByCouponCode(couponCode);

            discountcoupon.NumberOfTimesUsed = discountcoupon.NumberOfTimesUsed + 1;
            if (discountcoupon.NumberOfTimesUsed == discountcoupon.NumberOfTimesUsable)
            {
                discountcoupon.IsExpired = true;
            }

            discorepo.Save();

            return discountcoupon;
        }

        private bool VerifyRead(Quote quote)
        {
            if (quote == null)
            {
                return false;
            }

            if (!quote.CanUserRead(User.Identity.Name))
            {
                return false;
            }

            return true;
        }

        private bool VerifyWrite(Quote quote)
        {
            if (quote == null)
            {
                return false;
            }

            if (!quote.CanUserEdit(User.Identity.Name))
            {
                return false;
            }

            return true;
        }
    }
}