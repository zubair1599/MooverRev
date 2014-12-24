using System;
using System.Linq;
using System.Web.Mvc;
using Business.Enums;
using Business.Repository;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;

namespace MooversCRM.Controllers
{
    public class StatsController : BaseControllers.BaseController
    {
        public ActionResult Index()
        {
            return RedirectToAction("Index2");
        }

        public ActionResult Index2()
        {
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = start.AddMonths(1);
            if (DateTime.Now.Day < 5)
            {
                start = DateTime.Now.AddMonths(-1);
            }

            var repo = new FranchiseRepository();
            var franchise = repo.GetStorage();

            var json = new EmployeeRepository().GetStats(franchise.FranchiseID, start, end);
            var quoteRepo = new QuoteRepository();
            var quotesWon = quoteRepo.GetWonBetween(franchise.FranchiseID, start, end)
                .Where(q => q.PricingTypeID == (int)QuotePricingType.Binding)
                .Where(q => 
                    !q.Schedules.Any(s => s.Postings.Any(p => p.Posting_Employee_Rel.Any(e => e.Commission > 0)))
                ).Distinct().ToList();


            var totalHours = quotesWon.Sum(i => i.Postings.Sum(p => p.GetManHours()));
            var averageRating = 0m;
            if (totalHours > 0)
            {
                averageRating = quotesWon.Sum(i => i.GuaranteeData.GuaranteedPrice) / totalHours;
            }

            var things = new Business.JsonObjects.EmployeeStatJson() {
                IsAverage = true,
                Average = averageRating,
                ID = "COMPANY AVERAGE",
                Name = "COMPANY AVERAGE",
                Rates = quotesWon.Select(i => i.GuaranteeData.GuaranteedPrice)
            };

            var tmp = json.Stats.ToList();
            tmp.Add(things);
            json.Stats = tmp.OrderByDescending(i => i.GetAverage());
            return View(json);
        }
    }
}