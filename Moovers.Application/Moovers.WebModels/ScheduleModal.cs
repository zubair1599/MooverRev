using System;
using Business.Repository;
using Business.Repository.Models;

namespace Moovers.WebModels
{
    public struct ScheduleModal
    {
        public Guid quoteid { get; set; }

        public DateTime? day { get; set; }

        public int crew { get; set; }

        public int? hours { get; set; }

        public int? minutes { get; set; }

        public int? rangestart { get; set; }

        public int? rangeend { get; set; }

        public int? movers { get; set; }

        public string name { get; set; }

        public string paymentType { get; set; }

        public string cardnumber { get; set; }

        public string billingzip { get; set; }

        public string expirationmonth { get; set; }

        public string expirationyear { get; set; }

        public string cvv2 { get; set; }

        /// <summary>
        /// TODO: THis really doesn't belong here at all
        /// </summary>
        public void UpdateSchedule(ref Business.Models.Schedule schedule)
        {
            var quoteRepo = new QuoteRepository();
            var quote = quoteRepo.Get(this.quoteid);

            if (this.quoteid != Guid.Empty)
            {
                schedule.QuoteID = quoteid;
            }

            if (this.day.HasValue)
            {
                schedule.Date = this.day.Value;
            }

            if (this.crew > 0)
            {
                var repo = new CrewRepository();
                var forDayLookup = repo.GetForDayLookup(this.day.Value, this.crew, quote.FranchiseID);
                if (this.crew > 0 && forDayLookup == null)
                {
                    forDayLookup = new Business.Models.Crew(this.day.Value, this.crew, quote.FranchiseID);
                    repo.Add(forDayLookup);
                    repo.Save();
                }
            }

            if (this.crew != default(int))
            {
                var franchiseID = quote.FranchiseID;
                schedule.AddCrew(this.crew, franchiseID);
            }

            if (this.rangestart.HasValue && this.rangeend.HasValue)
            {
                var end = Math.Max(this.rangestart.Value, this.rangeend.Value);
                schedule.StartTime = this.rangestart.Value;
                schedule.EndTime = end;
            }

            if (this.movers.HasValue)
            {
                schedule.Movers = this.movers.Value;
            }

            if (this.minutes.HasValue)
            {
                schedule.Minutes = ((hours ?? 0) * 60) + this.minutes.Value;
            }
        }
    }
}
