using System;
using Business.Enums;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;

namespace Business.Models
{
    public partial class Posting_Employee_Rel
    {
        public Posting_Employee_Rel() { }

        public Posting_Employee_Rel(Employee_Rel rel) : this()
        {
            var repo = new EmployeeRepository();
            var emp = repo.Get(rel.EmployeeID);

            this.EmployeeID = rel.EmployeeID;
            this.Wage = emp.Wage ?? 0;
            this.Tip = rel.Tip ?? 0;
            this.PerDiem = rel.PerDiem ?? 0;
            this.Bonus = rel.Bonus ?? 0;
            this.Commission = rel.Commission ?? 0;
            this.Hours = rel.Hours ?? 0;
            this.IsRemoved = rel.IsRemoved;
        }

        public Employee_Rel ToJsonObject()
        {
            var r = this;
            return new Employee_Rel() {
                Employee = r.Employee.ToJsonObject(),
                EmployeeID = r.EmployeeID,
                Wage = r.Wage.ToString(),
                Tip = r.Tip,
                PerDiem = r.PerDiem,
                Bonus = r.Bonus,
                Commission = r.Commission,
                Hours = r.Hours,
                IsRemoved = r.IsRemoved,
                IsDriver = r.IsDriver,
                ForceNoDriver = r.ForceNoDriver
            };
        }

        public decimal GetCommission()
        {
            if (this.Posting.Quote.QuoteType == QuoteType.Local || this.Posting.Quote.QuoteType == QuoteType.Unset_Quote_Type)
            {
                var moveCost = this.Posting.GetMoveCost();
                var rate = (moveCost > 500) ? 0.24m : .20m;

                var totalHours = this.Posting.GetManHours();

                if (totalHours == 0)
                {
                    return 0;
                }

                return ((moveCost * rate) / totalHours) * this.Hours;
            }

            if (this.Posting.Quote.QuoteType == QuoteType.Regional)
            {
                var moveCost = this.Posting.GetMoveCost();
                var rate = (moveCost > 500) ? 0.24m : .20m;

                var totalHours = this.Posting.GetManHours();

                if (totalHours == 0)
                {
                    return 0;
                }

                return ((moveCost * rate) / totalHours) * this.Hours;
            }

            if (this.Posting.Quote.QuoteType == QuoteType.National)
            {
                var miles = this.Posting.Quote.GetDriveTime();
                var driverRate = Quote.LongDistanceDriverMileage;
                return (miles * driverRate) + Quote.LongDistanceDriverLoadUnload;
            }

            throw new InvalidOperationException();
        }
    }
}