namespace Moovers.WebModels
{
    public class ContractPrintModel
    {
        public Business.Models.Schedule Schedule { get; set; }

        public Business.Models.Quote Quote { get; set; }

        public bool IncludeCss { get; set; }

        public ContractPrintModel(Business.Models.Quote quote, Business.Models.Schedule schedule, bool includeCss)
        {
            this.Quote = quote;
            this.Schedule = schedule;
            this.IncludeCss = includeCss;
        }
    }
}
