namespace Moovers.WebModels
{
    public class AddCardModel
    {
        public string name { get; set; }

        public string cardnumber { get; set; }

        public string billingzip { get; set; }

        public string expirationmonth { get; set; }

        public string expirationyear { get; set; }

        public string cvv2 { get; set; }
    }
}
