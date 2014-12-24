using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public class PayrollSummaryModel : SortableModel<PayrollSummarySort>
    {
        public IEnumerable<KeyValuePair<Employee, IQueryable<Posting_Employee_Rel>>> Employees { get; set; }

        public IEnumerable<Quote> ValuationQuotes { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public override PayrollSummarySort Sort { get; set; }

        public override bool Desc { get; set; }

        public override IEnumerable<KeyValuePair<string, PayrollSummarySort>> GetHeaders()
        {
            var ret = new Dictionary<string, PayrollSummarySort> {
                { "Select", PayrollSummarySort.Employee },
                { "Employee", PayrollSummarySort.Employee }, 
                { "Valuation", PayrollSummarySort.Valuation }, 
                { "Comm", PayrollSummarySort.Comm }, 
                { "Tip", PayrollSummarySort.Tip }, 
                { "PerDiem", PayrollSummarySort.PerDiem }, 
                { "Hours", PayrollSummarySort.Hours }
            };

            return ret;
        }

        public string ToCsv()
        {
            var arr = Employees.Select(emp => new [] {   
                emp.Key.NameFirst, 
                emp.Key.NameLast, 
                String.Format("{0:C}", emp.Value.Any() ? emp.Value.Sum(i => i.Commission) : 0),
                String.Format("{0:C}", emp.Value.Any() ? emp.Value.Sum(i => i.Tip) : 0),
                String.Format("{0}", emp.Value.Any() ? emp.Value.Sum(i => i.Hours) : 0),
                 String.Format("{0}", emp.Value.Any() ? emp.Value.Sum(i => i.PerDiem) : 0)
            }).ToList();

            arr.Insert(0, new [] {
                "First Name", "Last Name", "Commission", "Tip", "Hours","Per Diem"
            });

            return String.Join("\n", arr.Select(i => String.Join(",", i)));
        }
    }

    public class EmployeeSummaryModel
    {
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public IEnumerable<Posting_Employee_Rel> Postings { get; set; }

        public IEnumerable<Quote> Valuations { get; set; }

        public Business.Models.Employee Employee { get; set; }
    }
}
