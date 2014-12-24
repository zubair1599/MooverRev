using System;
using System.Text;
using Business.Utility;
using System.Linq.Expressions;

namespace Business.Models
{
    public partial class QuoteSurvey
    {
        public QuoteSurvey()
        {
        }

        public QuoteSurvey(Guid quoteid, DateTime day, TimeSpan start, TimeSpan end)
            : this()
        {
            this.Created = DateTime.Now;
            this.IsCancelled = false;
            this.Date = day;
            this.Notes = String.Empty;
            this.QuoteID = quoteid;
            this.TimeStart = start;
            this.TimeEnd = end;
        }

        public string DisplayTime()
        {
            return Utility.Date.DisplayTimeSpan(this.TimeStart.Hours, this.TimeEnd.Hours);
        }


        public object ToJsonObject()
        {

            return new
            {

                Created = this.Created,
                IsCancelled = this.IsCancelled,
                Date = this.Date,
                Notes = this.Notes,
                QuoteID = this.QuoteID,
                TimeStart = this.TimeStart,
                TimeEnd = this.TimeEnd


            };
        }
    }
}
