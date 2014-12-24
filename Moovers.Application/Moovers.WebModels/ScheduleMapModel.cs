using System;
using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class ScheduleMapModel
    {
        public Business.Models.Address FranchiseAddress { get; set; }

        public IEnumerable<Quote> Quotes { get; set; }

        public DateTime Day { get; set; }

        public IEnumerable<Quote> GetPlot()
        {
            return this.Quotes;
        }

        //public IEnumerable<Models.Quote> GetUnplottable()
        //{
        //    return this.Quotes.Where(i => i.Stops.Any(s => !s.Address.IsVerified()));
        //}

        public ScheduleMapModel(Business.Models.Franchise franchise, IEnumerable<Quote> quotes, DateTime day)
        {
            this.Quotes = quotes;
            this.FranchiseAddress = franchise.Address;
            this.Day = day;
        }
    }
}
