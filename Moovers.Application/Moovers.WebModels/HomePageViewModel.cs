using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class HomePageViewModel
    {
        public IEnumerable<FrontPageMessage> Messages { get; set; }

        public IEnumerable<Quote> Quotes { get; set; }

        public IEnumerable<Quote> GoingToday { get; set; }

        public IEnumerable<QuoteSurvey> Surveys { get; set; }

        public Business.Models.Address FranchiseAddress { get; set; }

        public HomePageViewModel(IEnumerable<FrontPageMessage> messages, IEnumerable<Quote> quotes, IEnumerable<QuoteSurvey> survey,  IEnumerable<Quote> goingToday, Business.Models.Address franchiseAddress)
        {
            this.Messages = messages;
            this.Quotes = quotes;
            this.Surveys = survey;
            this.GoingToday = goingToday;
            this.FranchiseAddress = franchiseAddress;
        }
    }
}
