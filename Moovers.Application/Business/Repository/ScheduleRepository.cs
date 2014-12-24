using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.ToClean.QuoteHelpers;
using Business.Utility;

namespace Business.Repository.Models
{
    public class ScheduleRepository : RepositoryBase<Schedule>
    {
        private static readonly Func<MooversCRMEntities, Guid, DateTime, DateTime, IQueryable<Schedule>> CompiledGetBetween = CompiledQuery.Compile<MooversCRMEntities, Guid, DateTime, DateTime, IQueryable<Schedule>>(
            (db, franchiseid, start, end) => (from s in db.Schedules.Include("Quote").Include("Quote.Schedules").Include("Quote.Postings")
                where s.Quote.FranchiseID == franchiseid && s.Date >= start && s.Date <= end
                      && !s.IsCancelled
                select s)
            );

        private static readonly Func<MooversCRMEntities, Guid, DateTime, IQueryable<Schedule>> CompiledGetForDay = CompiledQuery.Compile<MooversCRMEntities, Guid, DateTime, IQueryable<Schedule>>(
            (db, franchiseid, day) => (db.Schedules.Include("Quote").Include("Postings").Where(s => s.Quote.FranchiseID == franchiseid && s.Date == day.Date && !s.IsCancelled))
            );

        private static readonly Func<MooversCRMEntities, Guid, Schedule> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Schedule>(
            (db, id) => db.Schedules.SingleOrDefault(i => i.ScheduleID == id)
            );

        public override Schedule Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public void ClearCrews(Schedule item)
        {
            var crews = (from c in db.Crew_Schedule_Rel
                where c.ScheduleID == item.ScheduleID
                select c);
            foreach (var c in crews)
            {
                db.Crew_Schedule_Rel.DeleteObject(c);
            }
        }

        public IEnumerable<Schedule> GetBetween(Guid franchiseID, DateTime start, DateTime end)
        {
            return CompiledGetBetween(db, franchiseID, start, end);
        }

        public IEnumerable<Schedule> GetForDay(Guid franchiseID, DateTime day)
        {
            return CompiledGetForDay(db, franchiseID, day);
        }

        public IQueryable<Schedule> GetUnprinted(Guid franchiseID, DateTime before, PostSortColumn sort, bool desc)
        {
            var schedules = (from s in db.Schedules
                where s.Date <= before.Date
                      && !s.IsCancelled
                      && !s.Postings.Any()
                      && s.Quote.FranchiseID == franchiseID
                select s);

            if (sort == PostSortColumn.AccountName)
            {
                schedules = schedules.Select(s => new {
                    schedule = s,
                    personaccount = db.Accounts.OfType<PersonAccount>().FirstOrDefault(a => a.AccountID == s.Quote.AccountID),
                    businessaccount = db.Accounts.OfType<BusinessAccount>().FirstOrDefault(a => a.AccountID == s.Quote.AccountID)
                }).OrderWithDirection(q => q.personaccount != null ? q.personaccount.LastName + " " + q.personaccount.FirstName : q.businessaccount.Name, desc).Select(q => q.schedule);
            }

            if (sort == PostSortColumn.Balance || sort == PostSortColumn.Employees || sort == PostSortColumn.Vehicles || sort == PostSortColumn.QuoteID)
            {
                schedules = schedules.OrderBy(s => s.QuoteID);
            }

            if (sort == PostSortColumn.PostingDate)
            {
                schedules = schedules.OrderBy(s => s.Date);
            }

            if (sort == PostSortColumn.Price)
            {
                schedules = schedules.OrderBy(s => s.Quote.PricingTypeID == (int)QuotePricingType.Hourly ?
                    (s.Quote.FirstHourPrice.Value + (s.Quote.CustomerTimeEstimate.Value - 1 + s.Quote.HourlyPrice.Value))
                    : s.Quote.GuaranteedPrice.Value);
            }

            if (sort == PostSortColumn.ServiceDate)
            {
                schedules = schedules.OrderBy(s => s.Date);
            }
            
            return schedules;
        }
    }
}