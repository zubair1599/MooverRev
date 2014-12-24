using System;
using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class QuoteSurveyModel : QuoteEdit
    {
        public Guid FranchiseID { get; set; }

        public DateTime Day { get; set; }

        public IEnumerable<QuoteSurvey> Surveys { get; set; }

        public QuoteSurveyModel(Guid franchiseID, Business.Models.Quote quote, IEnumerable<QuoteSurvey> surveys, DateTime date)
            : base(quote, "Schedule")
        {
            this.FranchiseID = franchiseID;
            this.Surveys = surveys;
            this.Day = date;
        }
    }
}
