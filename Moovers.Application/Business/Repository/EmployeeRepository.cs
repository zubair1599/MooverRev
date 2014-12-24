// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EmployeeRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Enums;
    using Business.JsonObjects;
    using Business.Models;
    using Business.Utility;
    using Business.ViewModels;

    using LinqKit;

    public class EmployeeRepository : RepositoryBase<Employee>
    {
        public override Employee Get(Guid id)
        {
            return db.Employees.SingleOrDefault(i => i.EmployeeID == id);
        }

        public IQueryable<Posting_Employee_Rel> GetJobs(Guid employeeid)
        {
            IQueryable<Posting_Employee_Rel> test = (from p in db.Posting_Employee_Rel.Include("Posting").Include("Posting.Schedule")
                where p.Posting.IsComplete && !p.IsRemoved && p.EmployeeID == employeeid
                select p);
            IQueryable<Posting_Employee_Rel> t2 = test.Where(c => c.PerDiem > 0);
            return test;
        }

        public IQueryable<Posting_Employee_Rel> GetJobs(Guid employeeid, DateTime start, DateTime end)
        {
            return
                (from j in GetJobs(employeeid)
                    where j.Posting.IsComplete && !j.IsRemoved && j.Posting.Schedule.Date >= start && j.Posting.Schedule.Date <= end
                    select j);
        }

        public Employee Get(string lookup)
        {
            return db.Employees.SingleOrDefault(i => i.Lookup == lookup);
        }

        public IEnumerable<Employee> GetWithoutLogins(Guid franchiseID)
        {
            return GetAll(franchiseID).Where(i => !i.EmployeeAuthorizations.Any());
        }

        public IOrderedQueryable<Employee> GetAll()
        {
            return (from emp in db.Employees where !emp.IsArchived select emp).OrderWithPadding(i => i.Lookup, 12, true);
        }

        public IOrderedQueryable<Employee> GetAll(Guid franchiseID)
        {
            return (from emp in db.Employees where emp.FranchiseID == franchiseID && !emp.IsArchived select emp).OrderWithPadding(i => i.Lookup, 12, true);
        }

        public IOrderedQueryable<Employee> GetAllIncludedArchived(List<Guid> franchiseIDs, EmployeeStatus status, int storenumber)
        {
            if (storenumber > 0)
            {
                return (from emp in db.Employees where franchiseIDs.Contains(emp.FranchiseID) && emp.Franchise.StoreNumber == storenumber && emp.StatusId == (int)status select emp).OrderWithPadding(i => i.Lookup, 12, true);
            }

            return (from emp in db.Employees where franchiseIDs.Contains(emp.FranchiseID) && emp.StatusId == (int)status select emp).OrderWithPadding(i => i.Lookup, 12, true);
        }

        public IOrderedQueryable<Employee> GetInactive(Guid franchiseID)
        {
            return (from emp in db.Employees where emp.FranchiseID == franchiseID && emp.IsArchived select emp).OrderWithPadding(i => i.Lookup, 12, true);
        }

        public IOrderedQueryable<Employee> GetAllIncludingArchived(Guid franchiseid)
        {
            return (from e in db.Employees where e.FranchiseID == franchiseid select e).OrderWithPadding(i => i.Lookup, 12, true);
        }

        public IEnumerable<KeyValuePair<Employee, IQueryable<Posting_Employee_Rel>>> GetWithPostings(Guid franchiseID, DateTime start, DateTime end)
        {
            return (from emp in db.Employees.Include("Posting_Employee_Rel").AsExpandable() where emp.FranchiseID == franchiseID select emp).ToDictionary(
                i => i,
                i => GetJobs(i.EmployeeID, start, end));
        }

        public StatsModel GetStats(Guid franchiseID, DateTime start, DateTime end)
        {
            var ret = new StatsModel { StartDate = start.ToShortDateString(), EndDate = end.ToShortDateString() };

            ret.Stats =
                GetAll(franchiseID)
                    .ToList()
                    .Select(
                        e =>
                            new { emp = e, jobs = GetJobs(e.EmployeeID, start, end).Where(i => i.Posting.Quote.PricingTypeID == (int)QuotePricingType.Binding) })
                    .Select(
                        e =>
                            new EmployeeStatJson()
                            {
                                ID = e.emp.Lookup,
                                Name = e.emp.DisplayShortName(),
                                Rates = e.jobs.Select(j => j.Hours).Where(i => i > 0),
                                Average = e.emp.GetManHourRateBetween(start, end, e.jobs)
                            });

            return ret;
        }
    }
}