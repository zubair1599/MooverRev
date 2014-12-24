using System;

namespace Business.ToClean.QuoteHelpers
{
    public struct GuaranteedInfo
    {
        public decimal BasePrice { get; set; }

        public decimal Adjustments { get; set; }

        public decimal GuaranteedPrice { get; set; }

        public decimal CalculateAdjustmentPercent()
        {
            if (this.BasePrice == 0)
            {
                return 0;
            }

            var test = Math.Round((this.Adjustments / this.BasePrice) * 100, 2);
            return test;
        }
    }
}