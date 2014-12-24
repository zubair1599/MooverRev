// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="QuotePricingModel.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.WebModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc.Html;

    using Business.Enums;
    using Business.Models;
    using Business.Repository.Models;
    using Business.ToClean.QuoteHelpers;

    public class QuotePricingModel : QuoteEdit
    {
        public static Dictionary<int, decimal> DestinationMultiplier = new Dictionary<int, decimal>() { { 60, 1m }, { 90, 1.6m }, { 120, 2.2m } };

        public QuotePricingModel()
        {
        }
        public string QuoteID { get; set; }
        public QuotePricingModel(Quote quote) : base(quote, "Pricing")
        {
            decimal weight = quote.GetWeight();
            QuoteID = quote.Lookup;
            //decimal moveprice = quote.HourlyData.FirstHourPrice + ((quote.HourlyData.CustomerTimeEstimate - 1) * quote.HourlyData.HourlyPrice);
            ReplacementValuationOptionsGuaranteed = GetReplacementValuationsForGuaranteed(weight);
            //ReplacementValuationOptionsHourly = GetReplacementValuationsForHourly(moveprice);
        }

        public QuotePricingType PricingType { get; set; }

        public int CrewSize { get; set; }

        public int Hours { get; set; }

        public int Trucks { get; set; }

        public IEnumerable<ReplacementValuation> ReplacementValuationOptionsGuaranteed { get; set; }

        public IEnumerable<ReplacementValuation> ReplacementValuationOptionsHourly { get; set; }

        public string GetTimeEstimate()
        {
            decimal hours = (Quote.GetQuicklook().TotalDuration / 60);
            return ((int)Math.Floor(hours)).ToString() + " - " + ((int)Math.Ceiling(hours)).ToString() + " Hours";
        }

        public int GetMinCrew()
        {
            return 2;
        }

        public int GetRecommendedCrew()
        {
            return Quote.GetMinimumMoversRequired();
        }

        public int GetMinTrucks()
        {
            return 1;
        }

        public int GetRecommendedTrucks()
        {
            return Quote.GetMinimumTrucksRequired();
        }

        public IEnumerable<ReplacementValuation> GetReplacementValuationsForGuaranteed(decimal weight)
        {
            var replacementValuationRepo = new ReplacementValuationRepository();
            List<ReplacementValuation> replacementValuationOptionsGuaranteedlocal = replacementValuationRepo.GetForWeight(weight).ToList();
            replacementValuationOptionsGuaranteedlocal.RemoveAll(type => type.Type == 4);
            decimal minimumcashbenefitforguaranteed = (((weight * 0.60m) * 1m) / 100m);
            replacementValuationOptionsGuaranteedlocal.Single(type => type.Type == 2).Cost = minimumcashbenefitforguaranteed < 10
                ? 10
                : minimumcashbenefitforguaranteed;
            return replacementValuationOptionsGuaranteedlocal;
        }

        public IEnumerable<ReplacementValuation> GetReplacementValuationsForHourly(decimal moveprice)
        {
            var replacementValuationRepo = new ReplacementValuationRepository();
            List<ReplacementValuation> replacementValuationOptionsHourlylocal = replacementValuationRepo.GetAll().ToList();
            replacementValuationOptionsHourlylocal.RemoveAll(type => type.Type == 3);
            replacementValuationOptionsHourlylocal.Single(model => model.Type == 2).Cost = (moveprice * 5m) / 100m;
            replacementValuationOptionsHourlylocal.Single(model => model.Type == 4).Cost = (moveprice * 15m) / 100m;
            return replacementValuationOptionsHourlylocal.OrderBy(model => model.Type);
        }


        public object ToJsonObject()
        {
            return new
            {
                QuoteID = this.QuoteID,
                PricingType = this.PricingType,
                CrewSize = this.CrewSize,
                Hours = this.Hours,
                Trucks = this.Trucks,
                ReplacementValuationOptionsGuaranteed = this.ReplacementValuationOptionsGuaranteed.OrderBy(i => i.MaximumValue).Select(m=>new { Cost = m.Cost , 
                    DisplayCost =  m.DisplayCost() , MaximumValue= m.MaximumValue ,
                    Name = m.Name , PerPound= m.PerPound , Type = m.Type , ValuationTypeID= m.ValuationTypeID}),

                //ReplacementValuationOptionsHourly = this.ReplacementValuationOptionsHourly.OrderBy(i => i.MaximumValue).Select(m => new
                //{
                //    Cost = m.Cost,
                //    DisplayCost = m.DisplayCost(),
                //    MaximumValue = m.MaximumValue,
                //    Name = m.Name,
                //    PerPound = m.PerPound,
                //    Type = m.Type,
                //    ValuationTypeID = m.ValuationTypeID
                //}),

                TimeEstimate = this.GetTimeEstimate(),
                MinCrew = this.GetMinCrew(),
                RecommendedCrew = this.GetRecommendedCrew(),
                MinTrucks = this.GetMinTrucks(),
                RecommendedTrucks = this.GetRecommendedTrucks(),
                QuotePriceDetails = new
                {
                    Vaults = this.Quote.CalculateStorageVaults(),
                    ServiceCost = this.Quote.GetServiceCost(ServiceType.PackingMaterials),
                    HasStorage = this.Quote.HasStorage(),
                    GuaranteedPrice = (this.Quote.PricingType == QuotePricingType.Hourly) ? this.Quote.CalculateGuaranteedPrice() : this.Quote.GuaranteeData.BasePrice ,
                    TotalTime= this.Quote.GetTotalTime()/60 ,
                    PricingType = this.Quote.PricingType,
                    PricingTrucks = this.Quote.GetPricingTrucks(),
                    CrewSize = this.Quote.CrewSize,
                    HasOldStorage = this.Quote.HasOldStorage,
                    HasTemporaryStorage = this.Quote.HasTemporaryStorage() ,

                    StorageCost = this.Quote.GetStorageCost(),
                    ForcedStorageCost = this.Quote.ForcedStorageCost,
                    MonthlyStorageCost = this.Quote.CalculateMonthlyStorageCost() ,
                    FinalPostedPrice = this.Quote.FinalPostedPrice,
                    EstimatedPriceWithServices = this.Quote.GetEstimatedPriceWithServices(),
                    QuoteID = this.Quote.QuoteID,
                    GuaranteeData = (this.Quote.PricingType == QuotePricingType.Binding) ? this.Quote.GuaranteeData : default(GuaranteedInfo),
                    HourlyData = this.Quote.PricingType == QuotePricingType.Hourly ? this.Quote.HourlyData : default(HourlyInfo),
                    Trucks = this.Quote.Trucks,
                     ESTIMATED_SOURCE_TIME=this.Quote.GetDriveTime() - this.Quote.GetDriveTime(false),
                    ESTIMATED_TRAVEL_TIME= this.Quote.GetDriveTime(),
                    ESTIMATED_MOVE_TIME= this.Quote.GetTotalTime(),
                    ESTIMATED_MILES= this.Quote.GetTotalMileage(),

                    PERSON_PRICE_MULTIPLIER= this.Quote.GetHourlyPersonMultiplier(),
                    TRUCK_PRICE_MULTIPLIER= this.Quote.GetHourlyTruckMultiplier(),
                    PERSON_DESTINATION_MULTIPLIER=this.Quote.GetHourlyPersonDestinationMultiplier(),
                    TRUCK_DESTINATION_MULTIPLIER=this.Quote.GetHourlyTruckDestinationMultiplier(),
                    CURRENT_HOURS=(this.Quote.PricingType == QuotePricingType.Hourly ? this.Quote.HourlyData.CustomerTimeEstimate : 0),
                    CURRENT_CREW= (this.Quote.CrewSize ?? 0),
                    CURRENT_TRUCKS=  (this.Quote.Trucks ?? 0),
                    TYPE= this.Quote.PricingType.ToString(),
                    CURRENT_FIRST_HOUR= (this.Quote.PricingType == QuotePricingType.Hourly ? this.Quote.HourlyData.FirstHourPrice : 0m),
                    CURRENT_HOUR= (this.Quote.PricingType == QuotePricingType.Hourly ? this.Quote.HourlyData.HourlyPrice : 0m),
                    DISCOUNTCOUPONUSED= (this.Quote.DiscountCopounUsed.ToString().ToLower()),
                    //DISCOUNTCOUPONCODE= (this.Quote.DiscountCouponCode != null ?? this.Quote.DiscountCouponCode: null)
                    
                }
            };
        }
    }
}