using System;
using System.Text.RegularExpressions;

namespace Business.Utility
{
    public static class Date
    {
        public static readonly DateTime SmallDatetimeMin = new DateTime(1753, 1, 1);

        private static DateTime firstPayroll = new DateTime(2013, 1, 4).AddMilliseconds(-1);

        private static DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        /// <summary>
        /// For dates in the current year, returns 3-letter month + year,
        /// for other dates, returns mm/dd/yyyy
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string GetShortDisplayDate(DateTime date)
        {
            if (date.Year == DateTime.Now.Year)
            {
                return date.ToString("MMM dd");
            }

            return date.ToShortDateString();
        }

        public static DateTime GetPayPeriodStart(DateTime date)
        {
            var between = new TimeSpan(date.Ticks - firstPayroll.Ticks);
            var payPeriods = Math.Floor(between.Days / 14d);
            return firstPayroll.AddDays((payPeriods) * 14d).AddMilliseconds(1);
        }

        public static DateTime GetPayPeriodEnd(DateTime date)
        {
            var between = new TimeSpan(date.Ticks - firstPayroll.Ticks);
            var payPeriods = Math.Floor(between.Days / 14d);
            return firstPayroll.AddDays((payPeriods + 1) * 14d).AddMilliseconds(-1);
        }

        /// <summary>
        /// converts searches like "Today", "Yesterday", or "Friday" into a datetime
        /// </summary>
        public static Tuple<DateTime, DateTime> GetDatesFromSearch(string search)
        {
            Func<DateTime, Tuple<DateTime, DateTime>> getFullDay = (d) => new Tuple<DateTime, DateTime>(d.Date, d.Date.AddDays(1).AddMilliseconds(-1));

            search = (search ?? String.Empty).ToLower().Trim();
            if (search == "today")
            {
                return getFullDay(DateTime.Today);
            }
            
            if (search == "yesterday")
            {
                return getFullDay(DateTime.Today.AddDays(-1));
            }

            if (search == "tomorrow")
            {
                return getFullDay(DateTime.Today.AddDays(1));
            }

            // Catch "Monday, Tuesday, Wednesday, Thursday, Friday"
            foreach (var day in Enum.GetNames(typeof(DayOfWeek)))
            {
                if (search == day.ToLower())
                {
                    // start at today, go 1 day back until we find a match w/ the current search
                    var tmpDate = DateTime.Today;
                    while (tmpDate.DayOfWeek != (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day))
                    {
                        tmpDate = tmpDate.AddDays(-1);
                    }

                    return getFullDay(tmpDate);
                }
            }

            // match strings like "in the last 30 days", "in the next year"
            // NOTE: currently only matches on "day", "month", and "year"
            var timespanMatch = new Regex(@"\bin the (last|next) (\d*) ?(day|month|year)");
            var matches = timespanMatch.Match(search);

            if (matches.Success)
            {
                var direction = matches.Groups[1].Captures[0].Value;
                var number = matches.Groups[2].Length > 0 ? int.Parse(matches.Groups[2].Captures[0].Value) : 1;
                var timespan = matches.Groups[3].Captures[0].Value;
                bool isforward = (direction == "next");

                if (timespan == "day")
                {
                    if (isforward) {
                        return new Tuple<DateTime, DateTime>(DateTime.Today, DateTime.Today.AddDays(number).AddMilliseconds(-1));
                    }
                    
                    return new Tuple<DateTime, DateTime>(DateTime.Today.AddDays(-number), DateTime.Today.AddDays(1).AddMilliseconds(-1));
                }

                if (timespan == "month")
                {
                    if (isforward) {
                        return new Tuple<DateTime,DateTime>(DateTime.Today, DateTime.Today.AddMonths(number).AddMilliseconds(-number));
                    }
                    
                    return new Tuple<DateTime,DateTime>(DateTime.Today.AddMonths(-number), DateTime.Today.AddDays(1).AddMilliseconds(-1));
                }

                if (timespan == "year")
                {
                    if (isforward) {
                        return new Tuple<DateTime,DateTime>(DateTime.Today, DateTime.Today.AddYears(number).AddMilliseconds(-number));
                    }
                    
                    return new Tuple<DateTime,DateTime>(DateTime.Today.AddYears(-number), DateTime.Today.AddDays(1).AddMilliseconds(-1));
                }
            }

            // catch other parsable dates
            DateTime dateSearch;
            DateTime.TryParse(search, out dateSearch);
            if (dateSearch != default(DateTime))
            {
                return getFullDay(dateSearch);
            }

            return null;
        }

        public static decimal SecondsToMinutes(decimal seconds)
        {
            return (seconds / 60);
        }

        public static DateTime UnixTimestampToDateTime(double unixTimestamp)
        {
            return unixEpoch.AddSeconds(unixTimestamp).ToLocalTime();
        }

        public static double DateTimeToUnixTimestamp(DateTime date)
        {
            return (date - unixEpoch.ToLocalTime()).TotalSeconds;
        }

        public static string DisplayTimeSpan(int start, int end)
        {
            if (start == end)
            {
                return DisplayHour(start);
            }

            if ((start > 11 && end > 11) || (start < 12 && end < 12))
            {
                return (start > 12 ? start - 12 : start) + " - " + DisplayHour(end);
            }

            return DisplayHour(start) + " - " + DisplayHour(end);
        }

        /// <param name="full">12:00 am vs 12am</param>
        public static string DisplayHour(int hour, bool full = false)
        {
            var extra = (full) ? ":00" : String.Empty;

            if (hour < 12)
            {
                return hour.ToString() + extra + "am";
            }

            if (hour == 12)
            {
                return hour + extra + "pm";
            }

            if (hour == 0 || hour == 24)
            {
                return "12" + extra + "am";
            }

            return (hour - 12) + extra + "pm";
        }
    }
}