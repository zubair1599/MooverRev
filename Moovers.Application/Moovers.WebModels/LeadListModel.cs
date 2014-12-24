using System.Linq;
using Business.Models;

namespace Moovers.WebModels
{
    public class LeadListModel
    {
        public IQueryable<Lead> Leads { get; set; }

        public IQueryable<Quote> WebQuotes { get; set; }

        public LeadListModel(IQueryable<Lead> leads, IQueryable<Quote> quotes)
        {
            this.Leads = leads;
            this.WebQuotes = quotes;
            
        }
    }
}
