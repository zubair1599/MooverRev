namespace Moovers.WebModels
{
    public class QuoteOverviewModel : QuoteEdit
    {
        public QuoteOverviewModel()
        {
        }

        public QuoteOverviewModel(Business.Models.Quote quote) 
            : base(quote, "Overview") 
        {
        }
    }
}
