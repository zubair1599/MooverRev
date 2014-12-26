using Business.Enums;
using Business.Repository;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;
using Moovers.WebModels.Reports;
using MooversCRM.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MooversCRM.Controllers
{
    //[Authorize]
    [MenuDescription("Reports")]
    public class ReportController : BaseControllers.SecureBaseController
    {
        // GET: /Report/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DailyActivity(string franchise, DateTime? day)
        {
            var repo = new QuoteRepository();
            var franchiseRepo = new FranchiseRepository();
            Guid? franchiseID = null;
            var quotes = repo.GetActiveOn(day ?? DateTime.Today);
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseID = AspUser.Franchise_aspnetUser.First().FranchiseID;
                ViewBag.LimitedAccess = true;
            }
            else if (!String.IsNullOrEmpty(franchise))
            {
                var selectedFranchise = franchiseRepo.GetAll().FirstOrDefault(f => f.Name.Equals(franchise));
                if (selectedFranchise != null)
                {
                    franchiseID = selectedFranchise.FranchiseID;
                    ViewBag.franchise = selectedFranchise.Name;
                }
            }
            if (franchiseID.HasValue)
            {
                quotes =
                    quotes.Where(
                        i =>
                            i.FranchiseID == franchiseID &&
                            !(i.StatusID == (int) QuoteStatus.Lost && i.AccountManagerID == WebQuoteUserID) &&
                            i.StatusID != (int) QuoteStatus.Duplicate);
            }
            else
            {
                quotes =
                    quotes.Where(
                        i =>
                            !(i.StatusID == (int) QuoteStatus.Lost && i.AccountManagerID == WebQuoteUserID) &&
                            i.StatusID != (int) QuoteStatus.Duplicate);

            }

            return View(new DailyActivityReport(quotes, day ?? DateTime.Today));
        }

        public ActionResult Payment(DateTime? day, Guid? franchiseID = null)
        {
            var repo = new PaymentRepository();
            var toreport = day ?? DateTime.Now;
            return View(new PaymentReport(repo.GetForDay(SessionFranchiseID, toreport), toreport));
        }

        public ActionResult SalesProjection(string franchise="",string salesperson = "", Guid? franchiseID = null)
        {
            var repo = new QuoteRepository();
            var franchiseRepo = new FranchiseRepository();
            ViewBag.Person = salesperson;
            ViewBag.franchise = "All";
            ViewBag.LimitedAccess = false;
            var sales = new List<SalesProjection>();

            // franchise users can only see this report for their franchise
            if (!AspUser.HasMultipleFranchises())
            {
                franchiseID = AspUser.Franchise_aspnetUser.First().FranchiseID;
                ViewBag.LimitedAccess = true;
            }
            else if (!String.IsNullOrEmpty(franchise))
            {
                var selectedFranchise = franchiseRepo.GetAll().Where(f => f.Name.Equals(franchise)).FirstOrDefault();
                if (selectedFranchise != null)
                {
                    franchiseID = selectedFranchise.FranchiseID;
                    ViewBag.franchise = selectedFranchise.Name;
                }
            }
          
            // months to loop through -- start at last month, go to next 2 months
            for (var i = -1; i <= 2; i++)
            {
                var date = DateTime.Now.AddMonths(i);
                var month = date.Month;
                var year = date.Year;
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1);
                var quoted = repo.GetQuotedBetween(franchiseID, startDate, endDate);
                var booked = repo.GetBookedBetween(franchiseID, startDate, endDate);

                if (!String.IsNullOrEmpty(salesperson))
                {
                    quoted = quoted.Where(q => q.AccountManager.LoweredUserName == salesperson);
                    booked = booked.Where(q => q.AccountManager.LoweredUserName == salesperson);
                }

                sales.Add(new SalesProjection(quoted, booked, month, year));
            }

            return View(sales);
        }

        public ActionResult BookingRate(DateTime? start = null, DateTime? end = null)
        {
            start = start ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            end = end ?? start.Value.AddMonths(1).AddMilliseconds(-1);

            var db = new QuoteRepository().db;

            var quotes = (from q in db.Quotes
                          where q.Created >= start.Value && q.Created <= end.Value
                          select q);

            return View(new BookingRate(quotes, start.Value, end.Value));
        }

        public ActionResult ItemList()
        {
            var repo = new InventoryItemRepository();

            var custom = repo.GetCustomItemCounts();
            var nonCust = repo.GetItemCounts();

            var model = new InventoryItemReport(
                nonCust.Select(i => new InventoryItemReportItem() {
                    item = i.Key.Name,
                    Count = i.Value
                }),
                custom.Select(i => new InventoryItemReportItem() {
                    item = i.Key,
                    Count = i.Value
                }));

            return View(model);
        }

        public ActionResult Deposit()
        {
            var repo = new PaymentRepository();
            return View(repo.GetUndeposited());
        }
    }
}
