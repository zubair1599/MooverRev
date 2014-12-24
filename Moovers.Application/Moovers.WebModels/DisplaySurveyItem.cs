using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Moovers.WebModels
{
    public class DisplaySurveyItem : QuoteSurveyModel
    {
        public string ItemClass { get; set; }

        public string SurveyID { get; set; }

        public int Hour { get; set; }

        public IEnumerable<Stop> Stops { get; set; }

        public string DisplayTime()
        {
            return Business.Utility.Date.DisplayTimeSpan(this.Hour, this.Hour + 1);
        }

        public Business.Models.QuoteSurvey Survey { get; set; }

        public Business.Models.Stop BaseStop { get; set; }

        public bool IsScheduled()
        {
            return this.Survey != null;
        }

        public bool HasStop()
        {
            return this.BaseStop != null;
        }

        public Business.Models.Stop SurveyStop
        {
            get
            {
                return this.Stops.First();
            }
        }

        public DisplaySurveyItem(QuoteSurveyModel model, int hour, Business.Models.Stop baseStop)
            : base(model.FranchiseID, model.Quote, model.Surveys, model.Day)
        {
            var time = new TimeSpan(hour, 0, 0);
            var scheduled = model.Surveys.FirstOrDefault(s => s.TimeStart.Hours == time.Hours);
            var stops = ((scheduled != null) ? scheduled.Quote.GetStops() : Enumerable.Empty<Business.Models.Stop>()).ToList();
            var isthis = scheduled != null && scheduled.Quote.Lookup == model.Quote.Lookup;

            this.Survey = scheduled;
            this.Stops = stops;
            this.SurveyID = (scheduled != null ? scheduled.SurveyID.ToString() : "");
            this.ItemClass = (isthis) ? "warning" : "";
            this.Hour = hour;
            this.BaseStop = baseStop;
        }
    }
}
