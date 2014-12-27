// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ScheduleController.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace MooversCRM.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing.Printing;
    using System.Linq;
    using System.Web.Mvc;

    using Business.Enums;
    using Business.Models;
    using Business.Repository;
    using Business.Repository.Models;
    using Business.Utility;

    using Moovers.WebModels;

    using MooversCRM.Controllers.BaseControllers;

    public class ScheduleController : SecureBaseController
    {
        public ActionResult Index()
        {
            var repo = new CustomCrewCountRepository();
            IEnumerable<DateTime> crews = repo.GetAll(SessionFranchiseID);
            var model = new FullScheduleModel(crews);
            return View(model);
        }

        public ActionResult ViewDay(int day, int month, int year)
        {
            var date = new DateTime(year, month, day);
            var repo = new ScheduleRepository();
            IEnumerable<Schedule> scheduled = repo.GetForDay(SessionFranchiseID, date);
            var employeeRepo = new EmployeeRepository();
            var vehicleRepo = new VehicleRepository();

            var surveyRepo = new QuoteSurveyRepository();
            IEnumerable<QuoteSurvey> surveys = surveyRepo.GetForDay(SessionFranchiseID, new DateTime(year, month, day));
            var model = new QuoteScheduleModel(
                SessionFranchiseID,
                new Quote(),
                date,
                scheduled,
                employeeRepo.GetAllIncludingArchived(SessionFranchiseID),
                vehicleRepo.GetAll(SessionFranchiseID),
                surveys);
            return View(model);
        }

        public ActionResult Print(Guid[] invoice, Guid[] storage)
        {
            var repo = new ScheduleRepository();
            IEnumerable<Schedule> forInvoice = (invoice ?? Enumerable.Empty<Guid>()).Select(inv => repo.Get(inv)).Where(i => i != null);
            IEnumerable<Schedule> forStorage = (storage ?? Enumerable.Empty<Guid>()).Select(inv => repo.Get(inv)).Where(i => i != null);

            IEnumerable<Quote> quotes = forInvoice.Select(i => i.Quote);

            if (quotes.Any(model => model.ValuationTypeID == null))
            {
                quotes.Where(model => model.ValuationTypeID == null)
                    .ToList()
                    .ForEach((quote) => { quote.ValuationTypeID = Guid.Parse("46637499-6B5B-4354-9C08-BC375FF64850"); });
                repo.Save();

                forInvoice = (invoice ?? Enumerable.Empty<Guid>()).Select(inv => repo.Get(inv)).Where(i => i != null);
                quotes = forInvoice.Select(i => i.Quote);
            }

            var pdfs = new List<string>();
            bool hasCss = false;
            foreach (Schedule schedule in forInvoice)
            {
                string html = RenderViewToString("PDFS/_Contract", new ContractPrintModel(schedule.Quote, schedule, !hasCss));
                var file = new File("Contract - Quote " + schedule.Quote.Lookup, html, "application/pdf");
                schedule.Quote.AddFile(file);
                pdfs.Add(html);
                hasCss = true;
            }

            foreach (Schedule schedule in forStorage)
            {
                string html = RenderViewToString("PDFS/_Storage", schedule.Quote);
                var file = new File("Storage List - Quote " + schedule.Quote.Lookup, html, "application/pdf");
                schedule.Quote.AddFile(file);
                pdfs.Add(html);
            }

            foreach (Quote quote in quotes.Where(q => q.StorageWorkOrder_Quote_Rel.Any(r => r.StorageQuoteTypeID == (int)StorageQuoteType.StorageAccess)))
            {
                string html = RenderViewToString("PDFS/_StorageAccess", quote);
                var file = new File("Storage Access - Quote " + quote.Lookup, html, "application/pdf");
                quote.AddFile(file);
                pdfs.Add(html);
            }

            byte[] pdf = General.GeneratePdf("<!DOCTYPE html><html>" + String.Join(String.Empty, pdfs) + "</html>", PaperKind.Legal);
            foreach (Schedule schedule in forInvoice)
            {
                schedule.CreatePosting();
            }

            repo.Save();
            return File(pdf, "application/pdf");
        }

        public ActionResult Map(DateTime date)
        {
            var franchiseRepo = new FranchiseRepository();
            var repo = new ScheduleRepository();
            Franchise franchise = franchiseRepo.Get(SessionFranchiseID);
            IEnumerable<Schedule> scheduled = repo.GetForDay(franchise.FranchiseID, date);
            var model = new ScheduleMapModel(franchise, scheduled.Select(i => i.Quote).Distinct(), date);
            return View(model);
        }

        public ActionResult Add(string id)
        {
            var repo = new QuoteRepository();
            Quote quote = repo.Get(id);

            if (quote == null)
            {
                return HttpNotFound();
            }

            return View(quote);
        }

        [HttpPost]
        public ActionResult SaveOrder(int day, int month, int year, IEnumerable<Guid> scheduleid, CrewStatus[] crewstatus, int[] crewid, string redirect = null)
        {
            scheduleid = scheduleid ?? Enumerable.Empty<Guid>();

            var repo = new ScheduleRepository();
            List<Schedule> schedules = scheduleid.Select(i => repo.Get(i)).ToList();
            var date = new DateTime(year, month, day);

            foreach (Schedule s in schedules)
            {
                repo.ClearCrews(s);
            }

            for (int i = 0; i < schedules.Count; i++)
            {
                Schedule schedule = schedules[i];
                int crewlookup = crewid[i];

                var crewrepo = new CrewRepository();
                Crew crew = crewrepo.GetForDayLookup(schedule.Date, crewlookup, SessionFranchiseID);
                if (crew == null && crewlookup > 0)
                {
                    crew = new Crew(schedule.Date, crewlookup, SessionFranchiseID);
                    crewrepo.Add(crew);
                    crewrepo.Save();
                }

                if (crewlookup > 0)
                {
                    schedule.AddCrew(crewlookup, SessionFranchiseID);
                }
            }

            for (int i = 1; i <= crewstatus.Count(); i++)
            {
                CrewStatus status = crewstatus[i - 1];
                var crewrepo = new CrewRepository();
                Crew crew = crewrepo.GetForDayLookup(date, i, SessionFranchiseID);

                if (crew == null)
                {
                    crew = new Crew(date, i, SessionFranchiseID);
                    crewrepo.Add(crew);
                }

                crew.Status = status;
                crewrepo.Save();
            }

            repo.Save();

            if (!String.IsNullOrEmpty(redirect))
            {
                return Redirect(redirect);
            }

            return RedirectToAction("ViewDay", new { Controller = "Schedule", day = day, month = month, year = year });
        }

        public ActionResult ScheduleEmployees(
            int crewlookup,
            DateTime day,
            IEnumerable<Guid> employeeid,
            IEnumerable<Guid> vehicleid,
            IEnumerable<Guid> driver,
            string redirect = null)
        {
            var repo = new CrewRepository();
            Crew crew = repo.GetForDayLookup(day, crewlookup, SessionFranchiseID);

            if (crew == null)
            {
                crew = new Crew(day, crewlookup, SessionFranchiseID);
                repo.Add(crew);
            }

            employeeid = employeeid ?? Enumerable.Empty<Guid>();
            vehicleid = vehicleid ?? Enumerable.Empty<Guid>();
            driver = driver ?? Enumerable.Empty<Guid>();

            IEnumerable<Guid> toRemove = crew.GetEmployees().Select(i => i.EmployeeID).Except(employeeid);
            foreach (Guid emp in toRemove)
            {
                crew.RemoveEmployee(emp);
            }

            foreach (Guid emp in employeeid)
            {
                bool isDriver = driver.Contains(emp);
                crew.AddEmployee(emp, isDriver);
            }

            IEnumerable<Guid> toRemoveVehicle = crew.GetVehicles().Select(i => i.VehicleID).Except(vehicleid);
            foreach (Guid vehicle in toRemoveVehicle)
            {
                crew.RemoveVehicle(vehicle);
            }

            foreach (Guid vehicle in vehicleid)
            {
                crew.AddVehicle(vehicle);
            }

            repo.Save();
            return Redirect(redirect);
        }

        public ActionResult UnconfirmMove(Guid id, string redirect)
        {
            var repo = new ScheduleRepository();
            Schedule schedule = repo.Get(id);
            schedule.Unconfirm();
            repo.Save();
            return Redirect(redirect);
        }

        public ActionResult ConfirmMove(Guid id, string redirect)
        {
            var repo = new ScheduleRepository();
            Schedule schedule = repo.Get(id);
            schedule.ConfirmQuote("Confirmation Client - " + User.Identity.Name, Request.ServerVariables["REMOTE_ADDR"], Request.UserAgent);
            schedule.Quote.UpdateModifiedDate(AspUserID);
            repo.Save();
            return Redirect(redirect);
        }
    }
}