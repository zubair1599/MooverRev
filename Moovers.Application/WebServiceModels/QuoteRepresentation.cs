// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="QuoteRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Enums;
    using Business.Models;
    using Business.ToClean.QuoteHelpers;

    using WebGrease.Css.Extensions;

    public class QuoteWrapper
    {
        public QuoteWrapper(List<QuoteRepresentation> quote_list)
        {
            quotes = quote_list;
        }

        public List<QuoteRepresentation> quotes { get; set; }
    }

    public class QuoteRepresentation
    {
        public QuoteRepresentation(Quote quote)
        {
            ishourly = false;
            isguranteed = false;
            schedules = new List<ScheduleRepresentation>();
            stops = new List<StopRepresentation>();
            quote_items_history = new List<InventoryItemHistory>();
            quote_pricing_algo = new List<PricingAlgoHistory>();
            quote_id = quote.QuoteID.ToString();
            quote_number = quote.Lookup;
            franchise_name = quote.Franchise.Name;
            franchise_id = quote.FranchiseID.ToString();
            customer = new AccountRepresentation(quote.Account);
            man_hour_rate = quote.GetManHourRate();
            quote.Schedules.Where(s => !s.IsCancelled).ForEach(s => schedules.Add(new ScheduleRepresentation(s)));
            move_type = quote.PricingType.ToString();
            Schedule firstSchedule = quote.Schedules.FirstOrDefault();
            printed_comments = quote.PrintedComments;
            quote_status = quote.Status;
            furniture_count = quote.GetQuicklook().Furniture;
            boxes_count = quote.GetQuicklook().Boxes;
            have_inventory = quote.HasInventory();
            forced_storage_cost = quote.ForcedStorageCost;
            total_time = quote.GetTotalTime();
            total_drive_time = quote.GetDriveTime(true);
            total_drive_miles = quote.GetTotalMileage();
            minimum_movers_required = quote.GetMinimumMoversRequired();
            minimum_truck_required = quote.GetMinimumTrucksRequired();
            total_labour_time = quote.GetQuicklook().LaborDuration;
            //total_time = quote.GetTotalTime();
            if (quote.ReplacementValuation != null)
            {
                terms_and_conditions = GetTerms(quote);
                this.valuation = new ValuationRepresentation
                {
                    valuation_type_id = quote.ReplacementValuation.ValuationTypeID,
                    name = quote.ReplacementValuation.Name,
                    total_weight = quote.GetWeight(),
                    max_coverage = quote.ReplacementValuation.MaximumValue,
                    value = quote.GetWeight()*Convert.ToDecimal(quote.ReplacementValuation.PerPound),
                };
            }

            quote.Quote_SavedItemList_Rel.ForEach(i => quote_items_history.Add(new InventoryItemHistory(i)));
            quote.Quote_PricingAlgorithm_Rel.ForEach(a => quote_pricing_algo.Add(new PricingAlgoHistory(a)));
            if (quote.PricingType == QuotePricingType.Binding)
            {
                isguranteed = true;
                guranteed_info = new
                {
                    base_price = quote.GuaranteeData.BasePrice,
                    adjustments = quote.GuaranteeData.Adjustments,
                    guranteed_price = quote.GuaranteeData.GuaranteedPrice,
                    adjustment_percentage = quote.GuaranteeData.CalculateAdjustmentPercent()
                };
            }
            else if (quote.PricingType == QuotePricingType.Hourly)
            {
                ishourly = true;
                hourly_info = quote.HourlyData;
            }
            ///payment

            var payments = quote.GetPayments();

            var existingPayments = new List<PaymentListRepresentation>();

            if (payments != null)
                payments.ForEach(p =>
                {
                    var pay = new PaymentListRepresentation()
                    {
                        quoteid = quote.QuoteID,
                        transaction_id = p.TransactionID,
                        payment_date = p.Date,
                        method = p.PaymentType.ToString(),
                        amount = p.Amount,
                        processed_by = p.GetProcessedBy() != null ? p.GetProcessedBy().UserName : "",
                        success = !p.IsCancelled,
                        card = p.Account_CreditCard != null ? p.Account_CreditCard.DisplayCard() : "",
                        credit_card_last4 = p.Account_CreditCard != null ? p.Account_CreditCard.GetLast4() : "",
                        credit_card_expire =
                            p.Account_CreditCard != null ? p.Account_CreditCard.GetExpiration() : "",
                        check_no = p.CheckNumber,
                        card_type = p.Account_CreditCard != null ? p.Account_CreditCard.CardType : ""
                    };
                    existingPayments.Add(pay);
                });

            payment_info = existingPayments;
            last_updated_time = DateTime.UtcNow;
            quote.Stops.ForEach(
                s =>
                {
                    var stop = new StopRepresentation(s);
                    stops.Add(stop);
                });

            if (firstSchedule != null)
            {
                sort = firstSchedule.StartTime;
            }
        }

        private string GetTerms(Quote quote)
        {
            return quote.ReplacementValuation.PerPound > 5
                ? "<p class='contract-terms'>"
                  +
                  "I accept reimbursement equal to the Replacement Value of lost or damaged goods. I declare a total value of "
                  + @String.Format("{0:C}", quote.ReplacementValuation.MaximumValue)
                  + " or a maximum of "
                  + @String.Format("{0:C}", quote.ReplacementValuation.PerPound)
                  +
                  " times the estimated weight of each claimed article, whichever is lesser. I understand that the total reimbursement"
                  +
                  " for lost or damaged goods shall not exceed this declared value. Moovers reserves the right to repair any"
                  + " damaged goods in lieu of reimbursement or replacement.</p>"
                : "<p class='contract-terms'>"
                  +
                  "I accept reimbursement equal to the Minimum Value of lost or damaged goods (Does not cover personal proprty. ex. walls, doors, etc.). I declare a total value of "
                  + @String.Format("{0:C}", quote.CalculateMinimumValuationLimit())
                  +
                  " or a  maximum of sixty cents per pound, per article, times the estimated weight of each article, whichever is lesser."
                  +
                  " I understand that the total reimbursement for lost or damaged goods shall not exceed this declared value. Moovers reserves the right to repair any damaged"
                  +
                  " goods in lieu of reimbursement or replacement. I also agree to inspect <strong>all</strong> items over $100 in value for damage and/or defects "
                  +
                  "<strong>BEFORE</strong> the movers leave. If damage and/or defects are present, I agree to contact the office and file my claim <strong>BEFORE</strong> the movers leave</p>";
        }

        public QuoteRepresentation()
        {
        }

        public string quote_id { get; set; }

        public string quote_number { get; set; }

        public AccountRepresentation customer { get; set; }

        public decimal total_drive_time { get; set; }
        public decimal total_drive_miles { get; set; }
        public int minimum_truck_required { get; set; }
        public int minimum_movers_required { get; set; }
        public decimal total_labour_time { get; set; }

        public decimal total_time { get; set; }
        public QuoteStatus quote_status { get; set; }

        public string move_type { get; set; }

        public string printed_comments { get; set; }

        public int boxes_count { get; set; }

        public List<StopRepresentation> stops { get; set; }

        public int furniture_count { get; set; }

        public DateTime last_sync_time { get; set; }

        public DateTime last_updated_time { get; set; }

        public bool have_inventory { get; set; }
        public List<ScheduleRepresentation> schedules { get; set; }

        public string franchise_id { get; set; }

        public string franchise_name { get; set; }

        public int? forced_storage_cost { get; set; }

        public decimal man_hour_rate { get; set; }

        public object guranteed_info { get; set; }

        public HourlyInfo hourly_info { get; set; }

        public List<QuoteService> quote_services { get; set; }

        public int sort { get; set; }

        public List<InventoryItemHistory> quote_items_history { get; set; }

        public List<PricingAlgoHistory> quote_pricing_algo { get; set; }

        public ValuationRepresentation valuation { get; set; }

        public string terms_and_conditions { get; set; }

        public List<PaymentListRepresentation> payment_info { get; set; }

        public bool ishourly { get; set; }
        public bool isguranteed { get; set; }
    }
}