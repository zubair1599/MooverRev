using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Repository.Models;
using LinqKit;

namespace Business.Models
{
    public sealed partial class Schedule : Interfaces.IPosting
    {
        public Schedule()
        {
            this.DateCreated = DateTime.Now;
        }

        public object ToJsonObject()
        {
            return new
            {
                Note = this.Note,
                Movers = this.Movers,
                IsCancelled = this.IsCancelled,
                Date = this.Date,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                
            };
        }
        public object ToJsonObject(int crew)
        {
            return new
            {
                CustomerName = this.Quote.Account.DisplayName, 
                QuoteNumber = this.Quote.Lookup , 
                Origin = (this.Quote.Stops!=null && this.Quote.Stops.Count>0) ? this.Quote.Stops.OrderBy(m=>m.Sort).FirstOrDefault().Address.DisplayString() : null,
                Destination =(this.Quote.Stops!=null && this.Quote.Stops.Count>0) ? this.Quote.Stops.OrderBy(m => m.Sort).LastOrDefault().Address.DisplayString() :null,
                Price = this.Quote.FinalPostedPrice,
                PricePerTruck = this.Quote.GetPricePerTruck(),
                Note = this.Note,
                Movers = this.Movers,
                IsCancelled = this.IsCancelled,
                Date = this.Date,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                ScheduledOnCrew = this.ScheduledOnCrew(crew),
                Crew_Schedule_Rel = this.Crew_Schedule_Rel.Any()

            };
        }
        public void Cancel(Guid cancelledBy)
        {
            this.IsCancelled = true;
            this.DateCancelled = DateTime.Now;
            this.CancelledBy = cancelledBy;
        }

        public void CreatePosting()
        {
            if (this.Postings.Any())
            {
                return;
            }

            var post = new Posting()
            {
                QuoteID = this.QuoteID,
                ScheduleID = this.ScheduleID,
                IsComplete = false
            };

            this.Postings.Add(post);
        }

        public IEnumerable<Crew> GetCrews()
        {
            return this.Crew_Schedule_Rel.Select(i => i.Crew).OrderBy(i => i.Lookup);
        }
        public IEnumerable<Employee> GetCrewEmployees()
        {
            return this.Crew_Schedule_Rel.Select(c=>c.Crew).SelectMany(ce=>ce.Crew_Employee_Rel).Select(e=>e.Employee);
        }
        public string GetCrewEmployeeNames()
        {
            string names = string.Empty;
             this.Crew_Schedule_Rel.Select(c => c.Crew).SelectMany(ce => ce.Crew_Employee_Rel).Select(e => e.Employee).ForEach(
                e =>
                {
                    names = names +", " + e.DisplayName();
                });
            return names;
        }
        public string DisplayTime()
        {
            return Utility.Date.DisplayTimeSpan(this.StartTime, this.EndTime);
        }

        public void AddCrew(int lookup, Guid franchiseID)
        {
            if (!this.ScheduledOnCrew(lookup))
            {
                var crewRepo = new CrewRepository();
                var crew = crewRepo.GetForDayLookup(this.Date, lookup, franchiseID);
                var rel = new Crew_Schedule_Rel {
                    CrewID = crew.CrewID,
                    ScheduleID = this.ScheduleID
                };
                this.Crew_Schedule_Rel.Add(rel);
            }
        }

        public bool ScheduledOnCrew(int? lookup)
        {
            if (!lookup.HasValue)
            {
                return !this.GetCrews().Any();
            }

            return this.GetCrews().Any(i => i.Lookup == lookup);
        }

        public void Unconfirm()
        {
            this.IsConfirmed = false;
            this.ConfirmedBy = null;
            this.ConfirmedIP = null;
            this.ConfirmedUserAgent = null;
            this.DateConfirmed = null;
        }

        public void ConfirmQuote(string confirmedBy, string confirmedIp, string userAgent)
        {
            this.IsConfirmed = true;
            this.ConfirmedBy = confirmedBy;
            this.ConfirmedIP = confirmedIp;
            this.ConfirmedUserAgent = userAgent;
            this.DateConfirmed = DateTime.Now;
        }

        #region IPosting Members

        Guid Interfaces.IPosting.PostingID
        {
            get { return Guid.Empty; }
        }

        Schedule Interfaces.IPosting.Schedule
        {
            get
            {
                return this;
            }
        }

        DateTime? Interfaces.IPosting.DateCompleted
        {
            get
            {
                return null;
            }
        }

        IEnumerable<Employee> Interfaces.IPosting.GetEmployees()
        {
            return Enumerable.Empty<Employee>();
        }

        IEnumerable<Vehicle> Interfaces.IPosting.GetVehicles()
        {
            return Enumerable.Empty<Vehicle>();
        }

        Quote Interfaces.IPosting.Quote
        {
            get
            {
                return this.Quote;
            }
        }

        bool Interfaces.IPosting.IsComplete
        {
            get { return false; }
        }

        string Interfaces.IPosting.Lookup
        {
            get { return String.Empty; }
        }

        #endregion
    }
}
