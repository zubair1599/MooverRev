using Business.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public struct QuoteQuicklook
    {
        public decimal TotalMiles { get; set; }

        public decimal BaseMiles { get; set; }

        public decimal MoveMiles { get; set; }

        public int Furniture { get; set; }

        public int Boxes { get; set; }

        public decimal CubicFeet { get; set; }

        public decimal TotalWeight { get; set; }

        public decimal TotalDuration { get; set; }

        public decimal LaborDuration { get; set; }

        public decimal DriveDuration { get; set; }

        public decimal FinalPrice { get; set; }

        public decimal Discount { get; set; }

        public decimal OriginalPrice { get; set; }

        public decimal? FinalPostedPrice { get; set; }

        public bool IsPaid { get; set; }

        public decimal? Balance { get; set; }

        public int? CustomerTimeEstimate { get; set; }

        public decimal HourlyRate { get; set; }

        public bool IsHourly { get; set; }

        public int MinimumMovers { get; set; }

        public int MinimumTrucks { get; set; }

        public bool IsCompleted { get; set; }

        public bool ShowLowerTrucksWarning { get; set; }

        public int PricingTrucks { get; set; }
        public int? DiscountType { get; set; }
        public decimal? AdjustmentPercentage { get; set; }
    }
}
