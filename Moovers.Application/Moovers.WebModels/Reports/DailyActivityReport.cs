using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Moovers.WebModels.Reports
{
    public struct DailyActivityReportItem
    {
        public Business.Models.Account Account { get; set; }

        public Business.Models.Quote Quote { get; set; }

        public Business.Models.aspnet_User User { get; set; }
    }

    public class DailyActivityReport
    {
        public IEnumerable<DailyActivityReportItem> ReportItems { get; set; }

        public DateTime Date { get; set; }

        public DailyActivityReport(IEnumerable<Quote> quotes, DateTime day)
        {
            this.Date = day;

            this.ReportItems = quotes.Select(i => new DailyActivityReportItem() {
                Account = i.Account,
                Quote = i,
                User = i.AccountManager
            }).OrderBy(i => i.User.LoweredUserName).ThenBy(i => i.Account.DisplayNameLastFirst);
        }
    }
}
