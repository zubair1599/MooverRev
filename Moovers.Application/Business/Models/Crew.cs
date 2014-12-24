using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Enums;
using Business.Utility;

namespace Business.Models
{
    public partial class Crew
    {
        public CrewStatus Status
        {
            get { return (CrewStatus)this.StatusID; }
            set { this.StatusID = (int)value; }
        }

        public Crew()
        {
        }

        public Crew(DateTime day, int lookup, Guid franchiseid) : this()
        {
            this.SetDate(day);
            this.Lookup = lookup;
            this.FranchiseID = franchiseid;
        }

        public IEnumerable<Crew_Employee_Rel> GetEmployees()
        {
            return this.Crew_Employee_Rel.Where(i => !i.IsArchived).OrderByDescending(i => i.IsDriver).ThenBy(i => i.Employee.Lookup);
        }

        public IEnumerable<Vehicle> GetVehicles()
        {
            return this.Crew_Vehicle_Rel.Where(i => !i.IsArchived).OrderBy(i => i.Vehicle.Lookup).Select(i => i.Vehicle);
        }

        private void SetDate(DateTime date)
        {
            this.Day = date.Day;
            this.Month = date.Month;
            this.Year = date.Year;
        }

        public void ClearEmployees()
        {
            foreach (var rel in this.Crew_Employee_Rel)
            {
                rel.IsArchived = true;
            }
        }

        public void RemoveEmployee(Guid employeeid)
        {
            var rel = this.Crew_Employee_Rel.First(i => i.EmployeeID == employeeid && !i.IsArchived);
            rel.IsArchived = true;

            foreach (var schedule in this.Crew_Schedule_Rel.Select(i => i.Schedule))
            {
                foreach (var post in schedule.Postings.Where(i => !i.IsComplete))
                {
                    var emp_rel = post.Posting_Employee_Rel.FirstOrDefault(i => i.EmployeeID == employeeid);
                    if (emp_rel != null)
                    {
                        post.Posting_Employee_Rel.Delete(emp_rel);
                    }
                }
            }
        }

        public void RemoveVehicle(Guid vehicleid)
        {
            var rel = this.Crew_Vehicle_Rel.First(i => i.VehicleID == vehicleid && !i.IsArchived);
            rel.IsArchived = true;

            foreach (var schedule in this.Crew_Schedule_Rel.Select(i => i.Schedule))
            {
                foreach (var post in schedule.Postings.Where(i => !i.IsComplete))
                {
                    var veh_rel = post.Posting_Vehicle_Rel.FirstOrDefault(i => i.VehicleID == vehicleid);
                    if (veh_rel != null)
                    {
                        post.Posting_Vehicle_Rel.Delete(veh_rel);
                    }
                }
            }
        }

        public void AddVehicle(Guid vehicleid)
        {
            var rel = this.Crew_Vehicle_Rel.FirstOrDefault(i => i.VehicleID == vehicleid);
            if (rel != null && rel.IsArchived)
            {
                rel.IsArchived = false;
            }

            if (rel != null)
            {
                return;
            }

            rel = new Crew_Vehicle_Rel {
                VehicleID = vehicleid
            };
            this.Crew_Vehicle_Rel.Add(rel);
        }

        public void AddEmployee(Guid employeeid, bool isDriver = false)
        {
            var rel = this.Crew_Employee_Rel.FirstOrDefault(i => i.EmployeeID == employeeid);
            if (rel != null && rel.IsArchived)
            {
                rel.IsArchived = false;
            }

            if (rel != null)
            {
                rel.IsDriver = isDriver;
                return;
            }

            rel = new Crew_Employee_Rel {
                IsDriver = isDriver,
                EmployeeID = employeeid
            };
            this.Crew_Employee_Rel.Add(rel);
        }
    }
}
