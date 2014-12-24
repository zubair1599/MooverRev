using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class SurveyList
    {
        public IEnumerable<QuoteSurvey> Surveys { get; set; }

        public Business.Models.Address FranchiseAddress { get; set; }
    }
}
