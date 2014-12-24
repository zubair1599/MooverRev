using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class Quote
    {
        /// <summary>
        /// For quotes that are scheduled for multiple trucks, gets the estimated price "per day per truck"
        /// </summary>
        /// <returns></returns>
        public decimal GetPricePerTruck()
        {
            var crews = this.GetSchedules().Sum(i => i.GetCrews().Count());
            var price = this.GetDisplayPrice();
            if (crews == 0)
            {
                return price;
            }

            return price / crews;
        }

        /// <summary>
        /// For quotes that are scheduled for multiple days, gets the estimated price "per day".
        /// </summary>
        /// <returns></returns>
        public decimal GetPricePerDay()
        {
            var price = this.GetDisplayPrice();
            if (this.Schedules.Count() <= 1)
            {
                return price;
            }

            return (price / this.Schedules.Count(s => !s.IsCancelled));
        }

        /// <summary>
        /// Returns the uncancelled schedule for "Day"
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public Schedule GetScheduleForDay(DateTime day)
        {
            var schedules = this.GetSchedules();
            day = day.Date;
            return schedules.FirstOrDefault(i => i.Date.Date == day);
        }

        /// <summary>
        /// Gets all non-cancelled "Schedules"
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Schedule> GetSchedules()
        {
            return this.Schedules.Where(i => !i.IsCancelled);
        }

        /// <summary>
        /// Displays all scheduled dates
        /// </summary>
        /// <param name="format">Format string to apply to scheduled dates</param>
        /// <param name="connector">String joiner</param>
        /// <returns>string</returns>
        public string GetScheduleDisplay(string format = "ddd, MMM d, yyyy", string connector = "<br>")
        {
            if (!this.Schedules.Any())
            {
                return this.MoveDate.ToString(format);
            }

            var schedules = this.GetSchedules().Select(i => i.Date.Date).OrderBy(i => i).ToList();
            schedules.Add(DateTime.MaxValue);

            var rangeStart = schedules.First();
            var rangeEnd = schedules.First();
            var showConnector = false;
            var sb = new StringBuilder();

            foreach (var date in schedules)
            {
                if (date == rangeEnd || date.AddDays(-1) == rangeEnd)
                {
                    rangeEnd = date;
                    continue;
                }

                if (showConnector)
                {
                    sb.Append(connector);
                }

                sb.Append(rangeStart.ToString(format));

                if (rangeStart != rangeEnd)
                {
                    sb.Append(" - " + rangeEnd.ToString(format));
                }

                showConnector = true;
                rangeStart = rangeEnd = date;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Cancel all quote scheduled dates
        /// </summary>
        /// <param name="userid">Active User (for logging purposes)</param>
        public void CancelSchedules(Guid userid)
        {
            foreach (var schedule in this.Schedules)
            {
                schedule.Cancel(userid);
            }
        }

        /// <summary>
        /// Gets schedules that have active postings (NOTE: This only indicates whether they have been printed, it doesn't indicate whether the postings are complete)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Schedule> GetPostingSchedules()
        {
            return this.Schedules.Where(i => i.Postings.Any());
        }
    }
}
