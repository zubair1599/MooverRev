using System;
using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;

namespace Moovers.WebModels.Reports
{
    public class SalesProjection
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public IEnumerable<Quote> Quoted { get; set; }

        public IEnumerable<Quote> Booked { get; set; }

        public decimal GetPostedAmount(DateTime? day = null)
        {
            var done = this.Booked.Where(i => i.IsComplete());

            if (!day.HasValue)
            {
                return done.Sum(i => i.FinalPostedPrice ?? 0);
            }

            done = done.Where(i => i.GetSchedules().Any(s => s.Date.Date == day.Value.Date));
            return done.Sum(i => i.FinalPostedPrice.HasValue ? i.GetPricePerDay() : 0);
        }

        public decimal GetBookedAmount(DateTime? day = null)
        {
            var complete = this.GetPostedAmount(day);

            var booked = this.Booked.Where(i => !i.IsComplete());
            if (day.HasValue)
            {
                booked = booked.Where(i => i.GetSchedules().Any(s => s.Date == day.Value.Date));
                var offset = (booked.Sum(i => i.PricingType == QuotePricingType.Hourly
                ? i.HourlyData.EstimateTotalHourly() : (i.GuaranteeData.GuaranteedPrice / i.GetSchedules().Count())));
                return complete + offset;
            }
            var totalOffset = (booked.Sum(i => i.PricingType == QuotePricingType.Hourly
                ? i.HourlyData.EstimateTotalHourly() : (i.GuaranteeData.GuaranteedPrice / i.GetSchedules().Count()) * i.GetMoveDays(this.Month)
            ));
            return complete + totalOffset;
            
        }

        public SalesProjection(IEnumerable<Quote> quoted, IEnumerable<Quote> booked, int month, int year)
        {
            this.Quoted = quoted.ToList();
            this.Booked = booked.ToList();
            this.Year = year;
            this.Month = month;
        }
    }
}
