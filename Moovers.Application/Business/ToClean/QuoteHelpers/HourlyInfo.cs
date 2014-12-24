namespace Business.ToClean.QuoteHelpers
{
    public struct HourlyInfo
    {
        public decimal FirstHourPrice { get; set; }

        public decimal HourlyPrice { get; set; }

        public int CustomerTimeEstimate { get; set; }

        public decimal EstimateTotalHourly()
        {
            if (this.CustomerTimeEstimate <= 1)
            {
                return FirstHourPrice;
            }

            return ((CustomerTimeEstimate - 1) * HourlyPrice) + FirstHourPrice;
        }
    }
}