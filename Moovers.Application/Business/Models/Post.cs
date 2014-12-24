using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Enums;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;
using Business.Utility;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Business.Models
{
    public struct Employee_Rel
    {
        public object Employee { get; set; }

        public Guid EmployeeID { get; set; }

        public string Wage { get; set; }

        public decimal? Tip { get; set; }
        public decimal? PerDiem { get; set; }

        public decimal? Bonus { get; set; }

        public decimal? Commission { get; set; }

        public decimal? Hours { get; set; }

        public bool IsRemoved { get; set; }

        public bool IsDriver { get; set; }

        public bool ForceNoDriver { get; set; }
    }

    public struct Vehicle_Rel
    {
        public Guid VehicleID { get; set; }

        public object Vehicle { get; set; }

        public bool IsRemoved { get; set; }
    }

    public enum ServiceType
    {
        [Description("Packing Materials")]
        PackingMaterials = 0,

        [Description("Packing Services")]
        PackingServices = 1,

        [Description("Storage Fees")]
        StorageFees = 3,

        [Description("Other")]
        Other = 4
    }

    public partial class Posting : Interfaces.IPosting
    {
        public decimal GetManHours()
        {
            if (!this.GetEmployees().Any())
            {
                return 0;
            }

            return this.Posting_Employee_Rel.Where(i => !i.IsRemoved).Sum(i => i.Hours);
        }

        public IEnumerable<Employee> GetEmployees()
        {
            if (this.Posting_Employee_Rel.Any())
            {
                return this.Posting_Employee_Rel.Where(e => !e.IsRemoved).OrderBy(i => i.Employee.Lookup).Select(e => e.Employee);
            }

            var crews = this.Schedule.GetCrews();
            var crew_employees = crews.SelectMany(i => i.Crew_Employee_Rel);
            return crew_employees.Select(i => i.Employee);
        }

        public IEnumerable<Vehicle> GetVehicles()
        {
            if (this.Posting_Vehicle_Rel.Any())
            {
                return this.Posting_Vehicle_Rel.Where(e => !e.IsRemoved).OrderBy(i => i.Vehicle.Lookup).Select(v => v.Vehicle);
            }

            return this.Schedule.GetCrews().SelectMany(i => i.Crew_Vehicle_Rel).Select(v => v.Vehicle);
        }

        public void AddVehicle(Vehicle_Rel json)
        {
            var rel = this.Posting_Vehicle_Rel.FirstOrDefault(i => i.VehicleID == json.VehicleID);
            if (rel == null)
            {
                rel = new Posting_Vehicle_Rel();
                this.Posting_Vehicle_Rel.Add(rel);
                rel.VehicleID = json.VehicleID;
            }

            rel.PostingID = this.PostingID;
            rel.IsRemoved = json.IsRemoved;
        }

        public void RemoveVehicles()
        {
            this.Posting_Vehicle_Rel.DeleteAll();
        }

        public void RemoveEmployees()
        {
            this.Posting_Employee_Rel.DeleteAll();
        }

        public void AddEmployee(Employee_Rel json)
        {
            var rel = this.Posting_Employee_Rel.FirstOrDefault(i => i.EmployeeID == json.EmployeeID);
            if (rel == null)
            {
                rel = new Posting_Employee_Rel();
                this.Posting_Employee_Rel.Add(rel);
            }

            var crews = this.Schedule.GetCrews().SelectMany(i => i.GetEmployees());
            var isCrewDriver = crews.Any(i => i.IsDriver && i.EmployeeID == json.EmployeeID);

            rel.EmployeeID = json.EmployeeID;
            rel.Wage = decimal.Parse(json.Wage ?? "0");
            rel.Tip = json.Tip ?? 0;
            rel.PerDiem = json.PerDiem ?? 0;
            rel.Bonus = json.Bonus ?? 0;
            rel.Commission = json.Commission ?? 0;
            rel.Hours = json.Hours ?? 0;
            rel.IsRemoved = json.IsRemoved;
            rel.IsDriver = json.IsDriver;
            rel.ForceNoDriver = !json.IsDriver && isCrewDriver;
        }

        public void MarkComplete(Guid postedBy)
        {
            this.IsComplete = true;
            this.DateCompleted = DateTime.Now;
            this.CompletedBy = postedBy;

            if (!this.Schedule.IsCancelled && this.Schedule.Quote.GetSchedules().All(i => i.Postings.Any(p => p.IsComplete)))
            {
                this.Schedule.Quote.SetStatus(postedBy, QuoteStatus.Completed, "Posted");
            }
            else if (this.Schedule.IsCancelled && !this.Quote.GetSchedules().Any())
            {
                this.Schedule.Quote.SetStatus(postedBy, QuoteStatus.Cancelled, "Posted cancellation");
            }
        }

        public QuoteService AddCustomService(Guid id, string description, decimal amount)
        {
            var rel = this.Quote.QuoteServices.FirstOrDefault(i => i.ServiceID == id);
            if (rel == null)
            {
                rel = new QuoteService();
                this.Quote.QuoteServices.Add(rel);
            }

            rel.Description = description;
            rel.Type = (int)ServiceType.Other;
            rel.Price = amount;
            return rel;
        }

        public void AddService(ServiceType serviceType, decimal amount)
        {
            var rel = this.Quote.QuoteServices.FirstOrDefault(i => i.Type == (int)serviceType);
            if (rel == null)
            {
                rel = new QuoteService();
                this.Quote.QuoteServices.Add(rel);
            }

            rel.Type = (int)serviceType;
            rel.Price = amount;
            rel.Description = serviceType.GetDescription();
        }

        private IEnumerable<QuoteService> GetCustomServices()
        {
            return this.Quote.QuoteServices.Where(i => i.Type == (int)ServiceType.Other);
        }

        public decimal GetTotalServiceCost()
        {
            return this.Quote.QuoteServices.Sum(i => i.Price);
        }

        public decimal GetMoveCost()
        {
            if (this.PostingHours.HasValue && this.PostingRate.HasValue && this.PostingFirstHourRate.HasValue)
            {
                var hourly = (this.PostingHours.Value - 1) * this.PostingRate.Value;
                var firstHour = this.PostingFirstHourRate.Value;

                return hourly + firstHour;
            }
            
            if (this.Schedule.Quote.PricingType == QuotePricingType.Binding)
            {
                return this.Schedule.Quote.GuaranteeData.GuaranteedPrice;
            }

            return 0;
        }

        private decimal GetServiceCost(ServiceType serviceType)
        {
            var rels = this.Quote.QuoteServices.Where(i => i.Type == (int)serviceType);
            if (rels.Any())
            {
                return rels.First().Price;
            }

            return 0;
        }

        public decimal? GetManHourRate()
        {
            if (this.Schedule.Quote.PricingType == QuotePricingType.Hourly || !this.IsComplete)
            {
                return null;
            }

            var hours = 0m;
            foreach (var sib in this.Schedule.Quote.GetSchedules().SelectMany(p => p.Postings))
            {
                hours += sib.GetManHours();
            }

            if (hours == 0)
            {
                return null;
            }

            var price = this.Schedule.Quote.GuaranteeData.GuaranteedPrice;
            return (price / hours);
        }

        public object ToJsonObject(bool includeSiblings = true)
        {
            var siblings = this.Schedule.Quote.GetPostingSchedules().SelectMany(i => i.Postings).Where(i => i.PostingID != this.PostingID).Where(i => i != null);
            var cardOnFile = this.Quote.Account_CreditCard;

            return new {
                Date = this.Schedule.Date.ToShortDateString(),
                PostingID = this.PostingID,
                Lookup = this.Lookup,

                CardOnFile = (cardOnFile != null) ? cardOnFile.DisplayCard() : String.Empty,
                Siblings = (includeSiblings) ? siblings.Select(i => i.ToJsonObject(false)) : Enumerable.Empty<object>(),

                IsComplete = this.IsComplete,
                IsCancelled = this.Schedule.IsCancelled,

                IsHourly = this.Schedule.Quote.PricingType == QuotePricingType.Hourly,

                HourlyRate = (this.Schedule.Quote.PricingType == QuotePricingType.Hourly && !this.PostingRate.HasValue) ? this.Schedule.Quote.HourlyData.HourlyPrice 
                                : (this.Schedule.Quote.PricingType == QuotePricingType.Hourly) ? this.PostingRate.Value
                                : 0,
                HourlyPricing = (this.Schedule.Quote.PricingType == QuotePricingType.Hourly) ? this.Schedule.Quote.HourlyData : default(HourlyInfo),
                GuaranteedPricing = (this.Schedule.Quote.PricingType == QuotePricingType.Binding) ? this.Schedule.Quote.GuaranteeData : default(GuaranteedInfo),
                FinalPostedPrice = this.Schedule.Quote.FinalPostedPrice,
                PostingHours = this.PostingHours,
                DriveHours = this.DriveHours,

                ValuationCost = this.Schedule.Quote.GetReplacementValuationCost(),

                PackingServiceCost = this.GetServiceCost(ServiceType.PackingServices),
                PackingMaterialsCost = this.GetServiceCost(ServiceType.PackingMaterials),
                StorageFeesCost = this.GetServiceCost(ServiceType.StorageFees),
                OtherServices = this.GetCustomServices().Select(i => i.ToJsonObject()),

                Employee_Rels = this.Posting_Employee_Rel.Select(r => r.ToJsonObject()),
                Crew_Employee_Rels = this.Schedule.GetCrews().SelectMany(i => i.GetEmployees()).Where(e=>e.Employee.FranchiseID == this.Quote.FranchiseID).Select(i => i.ToJsonObject()),
                Crew_Vehicle_Rels = this.Schedule.GetCrews().SelectMany(i => i.GetVehicles()).Where(v=>v.FranchiseID == this.Quote.FranchiseID).Select(i => i.ToJsonObject()),
                Vehicle_Rels = this.Posting_Vehicle_Rel.Select(r => r.ToJsonObject()),

                CrewCount = this.CrewCount ?? this.Schedule.Crew_Schedule_Rel.Count(),

                HasMonthlyStorage = this.Quote.HasMonthlyStorage(),
                HasTemporaryStorage = this.Quote.HasTemporaryStorage(),
                HasOldStorage = this.Quote.HasOldStorage,

                StorageAccount = this.Quote.StorageWorkOrder_Quote_Rel.Any()
                    ? this.Quote.StorageWorkOrder_Quote_Rel.First().StorageWorkOrder.ToJsonObject().SerializeToJson()
                    : "".SerializeToJson(),

                HasStorageAccounts = (this.Quote.FranchiseID == new FranchiseRepository().GetStorage().FranchiseID),

                StorageDays = this.Quote.GetStops().Max(i => i.StorageDays) ?? 0
            };
        }
    }

    public partial class Posting_Vehicle_Rel
    {
        public Posting_Vehicle_Rel() { }

        public Posting_Vehicle_Rel(Vehicle_Rel rel) : this()
        {
            this.VehicleID = rel.VehicleID;
            this.IsRemoved = rel.IsRemoved;
        }

        public Vehicle_Rel ToJsonObject()
        {
            var r = this;
            return new Vehicle_Rel()
            {
                VehicleID = r.VehicleID,
                Vehicle = r.Vehicle.ToJsonObject(),
                IsRemoved = r.IsRemoved
            };
        }
    }
}
