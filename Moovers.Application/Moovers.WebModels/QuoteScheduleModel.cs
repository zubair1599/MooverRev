using System;
using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Repository.Models;

namespace Moovers.WebModels
{
    using Business.Utility;

    public class QuoteScheduleModel : QuoteEdit
    {
        public int GetMaxCrewToDisplay()
        {
            var custRepo = new CustomCrewCountRepository();
            var cust = custRepo.GetForDay(this.Date, this.FranchiseID);

            if (cust.HasValue)
            {
                return cust.Value;
            }

            var repo = new GlobalSettingRepository(FranchiseID);
            return repo.GetValue<int>(SettingTypes.MaxTrucks);
        }

        public int GetCrewCount()
        {
            var custRepo = new CustomCrewCountRepository();
            var cust = custRepo.GetForDay(this.Date, this.FranchiseID) ?? Int32.MinValue;
            var repo = new VehicleRepository();
            return Math.Max(repo.GetAll(this.FranchiseID).Count() + 2, cust);
        }

        public Business.Models.Crew GetForDayLookup(DateTime date, int crew)
        {
            var repo = new CrewRepository();
            return repo.GetForDayLookup(date, crew, this.FranchiseID);
        }

        public IEnumerable<QuoteSurvey> Surveys { get; set; }

        public IEnumerable<Schedule> ScheduledQuotes { get; set; }

        public IEnumerable<Employee> Employees { get; set; }

        public IEnumerable<Vehicle> Vehicles { get; set; }

        public Guid FranchiseID { get; set; }

        public DateTime Date { get; set; }

        public int Month { get; set; }

        public QuoteScheduleModel()
        {
        }

        public QuoteScheduleModel(Guid franchiseID, Business.Models.Quote quote, DateTime date)
            : base(quote, "Schedule")
        {
            this.Date = date;
            this.FranchiseID = franchiseID;
        }

        public QuoteScheduleModel(Guid franchiseID, Business.Models.Quote quote, int month, IEnumerable<Employee> employees, IEnumerable<Vehicle> vehicles) 
            : base(quote, "Schedule") 
        {
            this.FranchiseID = franchiseID;
            this.Date = DateTime.Now;
            this.Month = month;
            this.Employees = employees;
            this.Vehicles = vehicles;
        }

        public QuoteScheduleModel(Guid franchiseID, Business.Models.Quote quote, DateTime day, IEnumerable<Schedule> scheduled, IEnumerable<Employee> employees, IEnumerable<Vehicle> vehicles, IEnumerable<QuoteSurvey> surveys)
            : this(franchiseID, quote, day.Month, employees, vehicles)
        {
            this.Date = day;
            this.ScheduledQuotes = scheduled.ToList();
            this.Surveys = surveys;
        }





        public object ToJsonObject()
        {
             var list = Enumerable.Range(1, this.GetCrewCount()).ToList();
          

            list.Add(0);
          

            return
                new
                {
                    Month = this.Month,
                    Date = this.Date,
                    Vehicles = this.Vehicles.Select(m => m.ToJsonObject()).ToList(),
                    Employees = this.Employees.Select(m => m.ToJsonObject()).ToList(),
                    ScheduledQuotes = this.ScheduledQuotes != null ? this.ScheduledQuotes.Select(m => m.ToJsonObject()).ToList() : null,
                    Surveys = this.Surveys != null ? this.Surveys.Select(m => m.ToJsonObject()).ToList() : null,
                    MaxCrewToDisplay = this.GetMaxCrewToDisplay(),
                    HasCard = this.Quote.Account_CreditCard !=null,
                    Crews = list.Select((m, index) => new
                    {
                        Number = list[index],
                        CrewDetail = new
                        {

                            ID = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).CrewID.ToString() : null,
                            Day = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).Day.ToString() : null,
                            FranchiseID = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).FranchiseID.ToString() : null,
                            Lookup = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).Lookup.ToString() : null,
                            Month = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).Month.ToString() : null,
                            Status = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).Status.ToString() : null,
                            Year = (this.GetForDayLookup(this.Date, list[index]) != null) ? this.GetForDayLookup(this.Date, list[index]).Year.ToString() : null,
                            GetEmployees = (this.GetForDayLookup(this.Date, list[index]) != null) ? (this.GetForDayLookup(this.Date, list[index]).GetEmployees().Any() ?

                                this.GetForDayLookup(this.Date, list[index]).GetEmployees().Select(em => new
                                {
                                    IsDriver = em.IsDriver,
                                    NameLast = em.Employee.NameLast,
                                    DisplayName = em.Employee.DisplayName(),
                                    Lookup = em.Employee.Lookup
                                }) : null) : null,

                            GetVehicles = (this.GetForDayLookup(this.Date, list[index]) != null) ? (this.GetForDayLookup(this.Date, list[index]).GetVehicles().Any() ?

                                   this.GetForDayLookup(this.Date, list[index]).GetVehicles().Select(ve => new
                                    {
                                        ID = ve.VehicleID,
                                        Lookup = ve.Lookup,
                                        Name = ve.Name

                                    }) : null) : null
                        },
                        Schedules = this.ScheduledQuotes != null ? this.ScheduledQuotes.Where(q => q.ScheduledOnCrew(list[index]) || (list[index] == 0 && !q.Crew_Schedule_Rel.Any())).ToList().OrderBy(i => i.StartTime).ThenBy(i => i.EndTime).Select(ma => ma.ToJsonObject(list[index])) : null,
                        SchedulesPricePerTruck = this.ScheduledQuotes != null ? this.ScheduledQuotes.Where(q => q.ScheduledOnCrew(list[index]) || (list[index] == 0 && !q.Crew_Schedule_Rel.Any())).ToList().Sum(q => q.Quote.GetPricePerTruck()).ToString() : null,

                    })

                    //Quote = new
                    //{
                    //    QuoteID = this.Quote.QuoteID,
                    //    Schedule = this.Quote.GetScheduleForDay(this.Date).ToJsonObject(),
                    //   // this.Quote.GetSchedules().Any(s => s.Date == this.Date && s.ScheduledOnCrew(crew))

                    //}
        
                

            };



        }




    }




}
