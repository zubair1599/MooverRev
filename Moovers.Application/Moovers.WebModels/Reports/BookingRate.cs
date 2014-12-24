using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;
using Business.Repository.Models;

namespace Moovers.WebModels.Reports
{
    public class BookingRate
    {
        private IQueryable<Quote> Quotes;

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public IQueryable<Quote> GetQuoted(Guid userid)
        {
            return this.Quotes.Where(i => i.AccountManagerID == userid);
        }

        public IQueryable<Quote> GetCompleted(Guid userid)
        {
            return this.GetQuoted(userid).Where(i => i.Schedules.Any(s => !s.IsCancelled) && i.Schedules.All(s => s.IsCancelled || s.Postings.Any(p => p.IsComplete)));
        }

        public IQueryable<Quote> GetPending(Guid userid)
        {
            return this.GetQuoted(userid).Where(i => i.Schedules.Any(s => !s.IsCancelled) && !i.Schedules.All(s => s.Postings.Any(p => p.IsComplete)));
        }

        public IQueryable<Quote> GetTotal(Guid userid)
        {
            return this.GetCompleted(userid).Concat(this.GetPending(userid));
        }

        public IEnumerable<aspnet_User> GetUsers()
        {
            var repo = new aspnet_UserRepository();
            return repo.GetSalesPeople();
        }

        public BookingRate(IQueryable<Quote> quotes, DateTime start, DateTime end)
        {
            this.Quotes = quotes;
            this.Start = start;
            this.End = end;
        }
    }
}
