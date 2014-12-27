// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Quote.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Security;

    using Business.Enums;
    using Business.Interfaces;
    using Business.JsonObjects;
    using Business.Repository.Models;
    using Business.ToClean.QuoteHelpers;
    using Business.Utility;

    using SmartFormat;

    using Roles = Business.Roles;

    public partial class Quote : IPayable
    {
        /// <summary>
        ///     "Get Display Price" -- this function needs to be available as both an sql expression and as a locally defined
        ///     function.
        /// </summary>
        internal static readonly Expression<Func<Quote, decimal>> GetCostExpr = quote => // if the quote is posted, display the posted price
            quote.Postings.Any(p => p.IsComplete && !p.Schedule.IsCancelled) && quote.FinalPostedPrice.HasValue
                ? quote.FinalPostedPrice.Value
                
                // for hourly quotes, display "minimum of 1 hour" price
                : quote.PricingTypeID == (int)QuotePricingType.Hourly
                    ? (quote.CustomerTimeEstimate ?? 0) <= 1
                        ? quote.FirstHourPrice.Value
                        // else, display first hour price + hourly rate
                        : quote.FirstHourPrice.Value + (quote.CustomerTimeEstimate.Value - 1) * quote.HourlyPrice.Value
                    // guaranteed quotes
                    : quote.GuaranteedPrice.Value;

        public Quote()
        {
            Created = DateTime.Now;
            TotalAdjustments = 0;
            Status = QuoteStatus.Open;
        }

        public QuoteType QuoteType
        {
            get
            {
                if (!CachedOneWayMoveMiles.HasValue)
                {
                    return QuoteType.Unset_Quote_Type;
                }
                if (CachedOneWayMoveMiles.Value <= (int)QuoteType.Regional)
                {
                    return QuoteType.Local;
                }
                if (CachedOneWayMoveMiles.Value <= (int)QuoteType.National)
                {
                    return QuoteType.Regional;
                }

                return QuoteType.National;
            }
        }

        public QuoteStatus Status
        {
            get { return (QuoteStatus)StatusID; }

            private set { StatusID = (int)value; }
        }

        public QuotePricingType PricingType
        {
            get { return (QuotePricingType)PricingTypeID; }

            private set { PricingTypeID = (int)value; }
        }

        public HourlyInfo HourlyData
        {
            get
            {
                
                if (PricingType != QuotePricingType.Hourly)
                {
                    return new HourlyInfo();
                    
                }
                
                return new HourlyInfo()
                {
                    // ReSharper disable PossibleInvalidOperationException
                    // This being an "hourly" quote asserts that all of these fields are defined
                    FirstHourPrice = FirstHourPrice.Value,
                    HourlyPrice = HourlyPrice.Value,
                    CustomerTimeEstimate = CustomerTimeEstimate.Value
                    // ReSharper restore PossibleInvalidOperationException
                };
            }

            set
            {
                FirstHourPrice = value.FirstHourPrice;
                HourlyPrice = value.HourlyPrice;
                CustomerTimeEstimate = value.CustomerTimeEstimate;
                PricingType = QuotePricingType.Hourly;
            }
        }

        public GuaranteedInfo GuaranteeData
        {
            get
            {
                //TODO: Check with zubair
                //if (PricingType != QuotePricingType.Binding)
                //{
                //    throw new InvalidOperationException("Can't get guaranteed data for hourly move");
                //}

                return new GuaranteedInfo()
                {
                    // ReSharper disable PossibleInvalidOperationException
                    // This being a guaranteed quote asserts all of these fields are defined
                    Adjustments = TotalAdjustments.Value,
                    BasePrice = BasePrice.Value,
                    GuaranteedPrice = GuaranteedPrice.Value
                    // ReSharper restore PossibleInvalidOperationException
                };
            }

            set
            {
                TotalAdjustments = value.Adjustments;
                BasePrice = value.BasePrice;
                GuaranteedPrice = value.GuaranteedPrice;
                PricingType = QuotePricingType.Binding;

                if (FinalPostedPrice.HasValue && Status != QuoteStatus.Completed)
                {
                    FinalPostedPrice = value.GuaranteedPrice;
                }
            }
        }
        
        Guid IPayable.ID
        {
            get { return QuoteID; }
        }

        public decimal GetDefaultDestinationFee()
        {
            decimal truckPrice = (Trucks ?? 1) * GetHourlyTruckDestinationMultiplier();
            decimal moverPrice = (CrewSize ?? 2) * GetHourlyPersonDestinationMultiplier();
            return truckPrice + moverPrice;
        }

        public decimal GetReplacementValuationCost()
        {
            if (ReplacementValuationCost.HasValue)
            {
                return ReplacementValuationCost.Value;
            }

            if (ValuationTypeID.HasValue)
            {
                return GetReplacementValuationCost(ValuationTypeID.Value, PricingType);
            }

            return 0;
        }

        public decimal GetReplacementValuationCost(Guid valuationType, QuotePricingType pricingType)
        {
            var valuationRepo = new ReplacementValuationRepository();
            ReplacementValuation valuation = valuationRepo.Get(valuationType);

            switch (pricingType)
            {
                case QuotePricingType.Binding:
                {
                    return CalculateReplacementValuationCostForBinding(valuation);
                }
                case QuotePricingType.Hourly:
                {
                    return CalculateReplacementValuationCostForHourly(valuation);
                }
                default:
                {
                    return 0;
                }
            }
        }

        public string GetCustomContractTerms()
        {
            var ret = new List<string>();

            if (QuoteType == QuoteType.Regional || QuoteType == QuoteType.National)
            {
                ret.Add(
                    "Payment is due by Cashiers Check, made payable to Moovers, in two (2) installments: 50% due prior to loading and 50% due prior to unloading.");
            }

            if (GetMoveMileage() >= PricingAlgorithm.LongDistanceMoveMiles)
            {
                var counts = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
                string msg =
                    "Pricing is based on a maximum of {0} ({1}) {1:truckload|truckloads} with a Not-To-Exceed, Gross Vehicle Weight (GVW) limitation of 25,950 pounds when loaded. Any overage, even if shown in the Inventory section on the Bill of Lading, will be denied and not shipped.";
                int trucks = GetPricingTrucks();
                string formatted = Smart.Format(msg, counts[trucks], trucks);
                ret.Add(formatted);
            }

            if (PricingType == QuotePricingType.Hourly)
            {
                ret.Add(
                    "Hourly services are provided \"as is\" and billed on a time plus material basis. This is not a flat rate. Any estimation or representation of time is not guaranteed and may vary greatly based on many factors. By electing for hourly services, you irrevocably waive any right to make a claim and/or recover charges for disputes arising from actual time billed and/or estimated time represented.");
                ret.Add("Valpak Discount does not apply to hourly rates.");
            }

            return String.Join("\n", ret);
        }

        public decimal GetManHourRate()
        {
            if (PricingType == QuotePricingType.Binding && Postings.Sum(i => i.GetManHours()) > 0)
            {
                return GuaranteeData.GuaranteedPrice / Postings.Sum(i => i.GetManHours());
            }

            return 0;
        }

        public bool IsComplete()
        {
            return Schedules.Any() && Schedules.All(i => i.IsCancelled || i.Postings.Any(p => p.IsComplete));
        }

        public int GetMoveDays(int month)
        {
            return Schedules.Where(s => s.Date.Month == month && !s.IsCancelled).Count();
        }

        public void ConfirmQuote(string confirmedBy, string confirmedIp, string userAgent)
        {
            foreach (Schedule s in GetSchedules())
            {
                s.ConfirmQuote(confirmedBy, confirmedIp, userAgent);
            }
        }

        public int GetPricingTrucks()
        {
            if (GetMoveMileage() > PricingAlgorithm.LongDistanceMoveMiles && Trucks.HasValue)
            {
                return Trucks.Value;
            }

            return GetMinimumTrucksRequired();
        }

        public bool HasStorage()
        {
            if (GetStops().Any(i => i.StorageDays.HasValue && i.StorageDays.Value > 0))
            {
                return true;
            }

            return GetStops().Any(i => i.GetAllRoomsItems().Any(s => s.StorageCount > 0 && s.Count > 0));
        }

        public bool HasTemporaryStorage()
        {
            if (HasOldStorage)
            {
                return false;
            }

            if (GetStops().Any(i => i.StorageDays.HasValue && (i.StorageDays.Value > 7 || i.StorageDays.Value <= 0)))
            {
                return false;
            }

            if (GetServiceCost(ServiceType.StorageFees) > 0)
            {
                return true;
            }

            return (GetStops().Any(i => i.StorageDays.HasValue));
        }

        public bool HasMonthlyStorage()
        {
            return CalculateStorageVaults() > 0 && !HasTemporaryStorage();
        }

        public decimal GetTemporaryStorageCost()
        {
            if (!HasTemporaryStorage())
            {
                return 0;
            }

            List<Stop> stopDays = GetStops().Where(i => i.StorageDays.HasValue && i.StorageDays.Value > 0).ToList();

            // override "temporary storage" cost
            if (stopDays.Any() && ForcedStorageCost.HasValue)
            {
                return ForcedStorageCost.Value;
            }

            if (!stopDays.Any())
            {
                return 0;
            }

            int days = stopDays.Max(i => i.StorageDays.Value);
            var settingRepo = new GlobalSettingRepository(FranchiseID);
            var nightlyStorage = settingRepo.GetValue<int>(SettingTypes.OvernightStorage);

            if (PricingType == QuotePricingType.Hourly)
            {
                return nightlyStorage * days * Trucks.Value;
            }

            return nightlyStorage * days * GetPricingTrucks();
        }

        public decimal GetStorageCost()
        {
            // see MOOCRM-36 -- Quote #34215 was quoted as $154.84 storage before we switched to a different system, it was the only quote like this.
            // Note: After this, several other quotes needed different storage costs, so all of this changed.
            if (Lookup == "34215")
            {
                return 154.84m;
            }

            if (!HasStorage())
            {
                return 0;
            }

            if (HasOldStorage)
            {
                return GetServiceCost(ServiceType.StorageFees);
            }

            if (HasTemporaryStorage())
            {
                return GetTemporaryStorageCost();
            }

            return CalculateMonthlyStorageCost();
        }

        public QuoteQuicklook GetQuicklook()
        {
            IEnumerable<Room> rooms = Stops.SelectMany(s => s.Rooms);
            List<ItemCollection> allitems = rooms.SelectMany(r => r.GetItems()).ToList();
            IEnumerable<ItemCollection> furniture = allitems.Where(i => !i.Item.IsBox);
            IEnumerable<ItemCollection> boxes = allitems.Where(i => i.Item.IsBox);
            decimal adjustments = 0m;
            decimal guaranteedprice = 0m;
            decimal originalprice = 0m;

            List<Posting> postings = Schedules.SelectMany(i => i.Postings).ToList();
            if (postings.Any(p => p.IsComplete && Status != QuoteStatus.Open))
            {
                decimal price = FinalPostedPrice ?? postings.Where(i => i.IsComplete).Sum(i => i.GetTotalServiceCost());
                adjustments = 0;
                originalprice = guaranteedprice = price;
            }
            else if (PricingType == QuotePricingType.Hourly)
            {
                HourlyInfo hourlyData = HourlyData;
                adjustments = 0;
                decimal estimate = hourlyData.FirstHourPrice + ((hourlyData.CustomerTimeEstimate - 1) * hourlyData.HourlyPrice);
                guaranteedprice = originalprice = estimate;
            }
            else if (PricingType == QuotePricingType.Binding)
            {
                GuaranteedInfo guaranteedData = GuaranteeData;
                guaranteedprice = guaranteedData.GuaranteedPrice;
                adjustments = guaranteedData.CalculateAdjustmentPercent();
                originalprice = guaranteedData.BasePrice;
            }

            decimal addition = GetServiceCost(ServiceType.PackingMaterials) + GetServiceCost(ServiceType.PackingServices);
            if (HasTemporaryStorage())
            {
                addition += GetStorageCost();
            }

            if (ValuationTypeID.HasValue)
            {
                addition += GetReplacementValuationCost();
            }

            decimal laborTime = (PricingType == QuotePricingType.Binding) ? GetFurnitureTime() : 0;
            decimal driveTime = GetDriveTime();

            originalprice += addition;
            guaranteedprice += addition;

            return new QuoteQuicklook()
            {
                TotalMiles = GetTotalMileage(),
                MoveMiles = GetMoveMileage(),
                BaseMiles = GetTotalMileage() - GetMoveMileage(),
                Furniture = furniture.Sum(i => i.Count),
                Boxes = boxes.Sum(i => i.Count),
                CubicFeet = GetCubicFeet(),
                TotalWeight = GetWeight(),
                TotalDuration = GetTotalTime(), // laborTime + driveTime,
                LaborDuration = laborTime,
                DriveDuration = driveTime,
                Discount = this._DiscountPriority.Equals((int)DiscountType.DiscountbyPercentage) ? (decimal)this._AdjustmentPercentage : adjustments,
                FinalPrice = guaranteedprice,
                OriginalPrice = originalprice,
                FinalPostedPrice = (Status == QuoteStatus.Open) ? null : FinalPostedPrice,
                IsPaid = FinalPostedPrice.HasValue && GetBalance() <= 0,
                Balance = (FinalPostedPrice.HasValue) ? GetBalance() : (decimal?)null,
                CustomerTimeEstimate = CustomerTimeEstimate,
                HourlyRate = (PricingType == QuotePricingType.Hourly) ? HourlyData.HourlyPrice : 0,
                IsHourly = PricingType == QuotePricingType.Hourly,
                MinimumMovers = GetMinimumMoversRequired(),
                MinimumTrucks = GetMinimumTrucksRequired(),
                IsCompleted = IsComplete(),
                ShowLowerTrucksWarning = (PricingType == QuotePricingType.Binding) && (GetPricingTrucks() < GetMinimumTrucksRequired()),
                PricingTrucks = GetPricingTrucks(),
                DiscountType =  this._DiscountPriority , 
                AdjustmentPercentage = this._AdjustmentPercentage
                
            };
        }

        
        public decimal GetDisplayPrice()
        {
            Func<Quote, decimal> expr = GetCostExpr.Compile();
            return expr(this);
        }

        public bool HasPoolTable()
        {
            return GetStops().Any(s => s.Rooms.Any(r => r.Room_InventoryItems.Any(i => i.Count > 0 && i.InventoryItem.Name.Contains("Pool Table"))));
        }

        public bool HasPiano()
        {
            return GetStops().Any(s => s.Rooms.Any(r => r.Room_InventoryItems.Any(i => i.Count > 0 && i.InventoryItem.Name.Contains("Piano"))));
        }

        public bool HasInventory()
        {
            return GetStops().Any(s => s.Rooms.Any(r => r.Room_InventoryItems.Any(i => i.Count > 0)));
        }

        public decimal GetEstimatedPriceWithServices()
        {
            decimal price = 0m;
            if (PricingType == QuotePricingType.Hourly)
            {
                price = HourlyData.EstimateTotalHourly();
            }
            else if (PricingType == QuotePricingType.Binding)
            {
                price = GuaranteeData.GuaranteedPrice;
            }

            price += GetStorageCost();
            price += GetReplacementValuationCost();
            price += GetServiceCost(ServiceType.PackingMaterials);
            return price;
        }

        public QuoteComment AddComment(Guid userid, string text)
        {
            var comment = new QuoteComment { QuoteID = QuoteID, UserID = userid, Text = text };
            QuoteComments.Add(comment);
            return comment;
        }

        public IEnumerable<QuoteComment> GetComments()
        {
            return QuoteComments.OrderBy(i => i.Date);
        }

        public IEnumerable<KeyValuePair<string, string>> GetAlerts()
        {
            IEnumerable<InventoryItem> items = Stops.SelectMany(s => s.Rooms.SelectMany(r => r.Room_InventoryItems.Select(i => i.InventoryItem)));
            return (from item in items from alert in item.GetAlerts() select new KeyValuePair<string, string>(item.Name, alert)).ToList();
        }

        public Quote_Competitor_Rel AddCompetitor(Competitor competitor, string name)
        {
            var rel = new Quote_Competitor_Rel { CompetitorID = competitor.CompetitorID, Name = name };
            Quote_Competitor_Rel.Add(rel);
            return rel;
        }

        public IEnumerable<string> GetStatusLogs()
        {
            IEnumerable<KeyValuePair<DateTime, string>> tmp = new List<KeyValuePair<DateTime, string>>().AsEnumerable();

            tmp =
                tmp.Concat(
                    QuoteStatusLogs.Where(i => i.Status.HasValue && i.Status != (int)QuoteStatus.Cancelled)
                        .Select(i => new KeyValuePair<DateTime, string>(i.Date, ((QuoteStatus)i.Status).ToString())));

            tmp =
                tmp.Concat(
                    GetPayments()
                        .Where(i => !i.IsCancelled && i.Success)
                        .Select(i => new KeyValuePair<DateTime, string>(i.Date, String.Format("{0:C} Payment Received", i.Amount))));

            tmp =
                tmp.Concat(
                    Schedules.SelectMany(i => i.Postings)
                        .Where(i => i.DateCompleted.HasValue)
                        .Select(i => new KeyValuePair<DateTime, string>(i.DateCompleted.Value, "Posted")));

            var created = new List<KeyValuePair<DateTime, string>> { new KeyValuePair<DateTime, string>(Created, "Quote Created") };

            tmp = tmp.Concat(created);

            return tmp.OrderByDescending(i => i.Key).Select(i => i.Value + " - " + i.Key.ToShortDateString());
        }

        public void AddCard(Account_CreditCard card)
        {
            Account_CreditCard = card;
        }

        public void AddCard(Guid cardid)
        {
            CreditCardID = cardid;
        }

        public Quote_File_Rel AddFile(File f)
        {
            List<File> existing = GetFiles().ToList();
            int cur = 0;
            string origName = f.Name;
            while (existing.Any(i => i.Name == f.Name) && cur < 30)
            {
                cur++;
                f.Name = origName + " (" + cur + ")";
            }

            return new Quote_File_Rel { File = f, Quote = this };
        }

        public IEnumerable<EmailLog> GetEmails()
        {
            return EmailLogs.OrderBy(i => i.DateSent);
        }

        public IEnumerable<File> GetFiles()
        {
            return Quote_File_Rel.Select(i => i.File).OrderBy(i => i.Created);
        }

        /// <summary>
        ///     When we imported data from Movepoint, all inventory is stored in comma separated values.
        ///     This is a simple way to get a list of inventory from the imported data.
        /// </summary>
        public IEnumerable<string> GetMovepointInventory()
        {
            if (String.IsNullOrEmpty(MovepointInventory) || GetStops().Any(s => s.Rooms.Any()))
            {
                return Enumerable.Empty<string>();
            }

            return MovepointInventory.Split(',');
        }

        public IEnumerable<ItemCollection> GetItems()
        {
            return Stops.SelectMany(i => i.GetItems());
        }

        /// <summary>
        ///     Gets the stop by which a move's "Franchise" should be associated
        /// </summary>
        /// <returns></returns>
        public Stop GetAssignmentStop()
        {
            IEnumerable<Stop> stops = GetStops();
            return stops.First();
        }

        public IEnumerable<Stop> GetStops()
        {
            return Stops.OrderBy(i => i.Sort);
        }

        public Address GetFranchiseAddress()
        {
            //for old quotes get the old moovers office address
            return Created < Franchise.UpdateAddressDate ? Franchise.OldAddress : Franchise.Address;
        }

        public void OrderStops()
        {
            int sort = 0;
            List<Stop> stops = GetStops().OrderBy(i => i.Sort).ToList();
            foreach (Stop stop in stops)
            {
                stop.Sort = sort;
                sort++;
            }
        }

        public IEnumerable<QuoteSurvey> GetSurveys()
        {
            return QuoteSurveys.Where(i => !i.IsCancelled).OrderBy(i => i.Date).ThenBy(i => i.TimeStart);
        }

        /// <summary>
        ///     Gets a human readable string of survey times.
        /// </summary>
        /// <returns></returns>
        public string DisplaySurveys()
        {
            List<QuoteSurvey> surveys = GetSurveys().ToList();

            if (!surveys.Any())
            {
                return "";
            }

            if (surveys.All(i => i.Date == surveys.First().Date))
            {
                string date = GetSurveys().First().Date.ToShortDateString();
                TimeSpan startTime = GetSurveys().Min(i => i.TimeStart);
                TimeSpan endTime = GetSurveys().Max(i => i.TimeEnd);

                return date + " " + Date.DisplayTimeSpan(startTime.Hours, endTime.Hours);
            }

            return String.Join(", ", GetSurveys().Select(i => i.Date.ToShortDateString() + " " + i.DisplayTime()));
        }

        public QuoteAccessLog GetLastAccess()
        {
            return QuoteAccessLogs.OrderByDescending(i => i.Date).Where(i => i.aspnet_Users.LoweredUserName != "aaron").Skip(1).FirstOrDefault();
        }

        public bool CanUserRead(string username)
        {
            string[] roles = System.Web.Security.Roles.GetRolesForUser(username);
            if (Roles.IsCorporateUser(roles))
            {
                return true;
            }

            var repo = new aspnet_UserRepository();
            aspnet_User user = repo.Get(username);
            return user.HasPermissionsOn(FranchiseID);
        }

        public bool CanUserEdit(string username)
        {
            // if a quote is open and they can read it, they can edit it.
            bool hasPosted = Schedules.Any(s => s.Postings.Any() && s.Postings.All(p => p.IsComplete));
            if (hasPosted)
            {
                return false;
            }

            if (Status != QuoteStatus.Open && Status != QuoteStatus.Scheduled)
            {
                return false;
            }

            return CanUserRead(username);
        }

        /// <summary>
        ///     If a move is scheduled or completed, gets the first day it's scheduled for.
        ///     If a move isn't scheduled, gets the estimated move date.
        /// </summary>
        /// <returns></returns>
        public DateTime GetDisplayMoveDate()
        {
            if (GetSchedules().Any())
            {
                return GetSchedules().Min(i => i.Date);
            }

            return MoveDate;
        }

        /// <summary>
        ///     Gets a relatively fast "listing" json -- this function is svae to call on large collections of quotes
        /// </summary>
        /// <returns></returns>
        public object ToListingJsonObject()
        {
            DateTime moveDate = (GetSchedules().Any()) ? GetSchedules().Min(i => i.Date) : MoveDate;
            var aspUserRepo = new aspnet_UserRepository();
            return
                new
                {
                    QuoteID = QuoteID,
                    AccountLookup = Account.Lookup,
                    Lookup = Lookup,
                    DisplayName = Account.DisplayName,
                    DisplayMoveDate = Date.GetShortDisplayDate(moveDate),
                    DaysToMove = Math.Round(TimeSpan.FromTicks(moveDate.Ticks - DateTime.Today.Ticks).TotalDays),
                    Price = (PricingType == QuotePricingType.Binding) ? GuaranteeData.GuaranteedPrice : HourlyData.EstimateTotalHourly(),
                    LastModifiedUser = (ModifiedBy.HasValue) ? aspUserRepo.Get(ModifiedBy.Value).LoweredUserName : "",
                    DisplayLastModifiedDate = (DateModified.HasValue) ? Date.GetShortDisplayDate(DateModified.Value) : "",
                    Status = Status.GetDescription(),
                    AccountManager = AccountManager.UserName,
                    QuoteType = ((int)QuoteType == -1) ? "" : QuoteType.GetDescription(),
                    HasSurveys = GetSurveys().Any(),
                    IsHourly = PricingType == QuotePricingType.Hourly,
                    Unassigned = AccountManager.LoweredUserName == General.WebQuoteUser.ToLower(),
                    FranchiseLogo = Franchise.GetIconUrl()
                };
        }

        /// <summary>
        ///     Gets a comprehensive collection of data for a quote. This function is slow - only call this on small collections
        /// </summary>
        /// <param name="includeItems">Whether or not to include all item data for the quote</param>
        /// <returns></returns>
        public object ToJsonObject(bool includeItems = false)
        {
            return
                new
                {
                    Lookup = Lookup,
                    Created = Created.ToShortDateString(),
                    MoveDate = GetDisplayMoveDate().ToShortDateString(),
                    Price = GetDisplayPrice(),
                    Stage = Status.GetDescription(),
                    QuoteID = QuoteID,
                    Stops = Stops.Select(i => i.ToJsonObject(includeItems)),
                    Schedules = GetSchedules().Select(s => s.ToJsonObject()),
                    IsCompleted = !Schedules.Any(i => i.Postings.Any(p => p.IsComplete)),
                    AccountName = Account.DisplayName,
                    FranchiseLogo = Franchise.GetIconUrl()
                };
        }

        public void AddLog(Guid userid, string message, QuoteStatus? status = null)
        {
            QuoteStatusLogs.Add(new QuoteStatusLog() { Date = DateTime.Now, Message = message, Reason = "", Status = (int?)status, UserID = userid });
        }

        public void UpdateModifiedDate(Guid userid)
        {
            DateModified = DateTime.Now;
            ModifiedBy = userid;
        }

        public void ForceStatusUpdate(Guid userid, QuoteStatus status, string reason)
        {
            var log = new QuoteStatusLog() { Date = DateTime.Now, QuoteID = QuoteID, Reason = reason, Status = (int)status, UserID = userid };

            var logRepo = new QuoteStatusLogRepository();
            logRepo.Add(log);
            logRepo.Save();

            Status = status;
            ActionReason = reason;
            UpdateModifiedDate(userid);
            if (status == QuoteStatus.Duplicate && Account.BillingQuotes.Count() == 1 && Account.ShippingQuotes.Count() == 1)
            {
                Account.IsArchived = true;
            }
        }

        public void SetStatus(Guid userid, QuoteStatus status, string reason)
        {
            // once a quote has been completely posted, it can only be marked as "Won" or "Lost"
            if (Schedules.Any(i => i.Postings.Any(p => p.IsComplete)) && status != QuoteStatus.Completed && status != QuoteStatus.Cancelled)
            {
                throw new InvalidOperationException("Quotes cannot be changed once posted");
            }

            ForceStatusUpdate(userid, status, reason);
        }

        public Quote Duplicate(Guid userid, bool forStorage = false)
        {
            var ret = new Quote()
            {
                FranchiseID = FranchiseID,
                AccountID = AccountID,
                ShippingAccountID = ShippingAccountID,
                MoveDate = MoveDate,
                StatusID = (int)QuoteStatus.Open,
                ReferralMethod = ReferralMethod,
                PricingTypeID = PricingTypeID,
                AccountManagerID = userid,
                Created = DateTime.Now,
                Trucks = Trucks,
                CrewSize = CrewSize,
                BasePrice = BasePrice,
                TotalAdjustments = TotalAdjustments,
                GuaranteedPrice = GuaranteedPrice,
                FirstHourPrice = FirstHourPrice,
                HourlyPrice = HourlyPrice,
                CustomerTimeEstimate = CustomerTimeEstimate,
                DateScheduled = DateScheduled,
                PrintedComments = PrintedComments
            };

            ret.AddLog(userid, "Duplicated from Quote " + Lookup);

            if (forStorage)
            {
                List<ItemCollection> items = GetStops().SelectMany(s => s.GetItems().Where(i => i.StorageCount > 0)).ToList();
                foreach (ItemCollection i in items)
                {
                    i.Count = i.StorageCount;
                    i.StorageCount = 0;
                }

                var roomJson = new RoomJson()
                {
                    Boxes = Enumerable.Empty<BoxRelJson>(),
                    Description = "Moover's Storage",
                    Items = items,
                    Pack = false,
                    RoomID = String.Empty,
                    Sort = 0,
                    StopName = String.Empty,
                    Type = "Storage"
                };

                var room = new Room(roomJson);
                Stop newStop = Stop.GetStorageStop(Franchise, 0);
                newStop.AddressType = StopAddressType.MooversStorage;
                newStop.StorageDays = -1;
                newStop.Rooms.Add(room);
                ret.Stops.Add(newStop);
            }
            else
            {
                foreach (Stop item in GetStops().ToList())
                {
                    Stop stop = item.Duplicate();
                    ret.Stops.Add(stop);
                }
            }

            ret.AddComment(userid, "Copied from Quote " + Lookup);
            return ret;
        }

        public void AddService(ServiceType serviceType, decimal price)
        {
            QuoteService service = QuoteServices.FirstOrDefault(i => i.Type == (int)serviceType);
            if (service == null)
            {
                service = new QuoteService { QuoteID = QuoteID, Type = (int)serviceType, Description = serviceType.ToString() };
                QuoteServices.Add(service);
            }

            service.Price = price;
        }

        public decimal GetServiceCost(ServiceType serviceType)
        {
            QuoteService s = QuoteServices.FirstOrDefault(i => i.Type == (int)serviceType);
            return (s != null) ? s.Price : 0;
        }

        /// <summary>
        ///     Total Move Time (from Franchise location back to Franchise location, including furniture load/unload time)
        /// </summary>
        /// <returns></returns>
        public decimal GetTotalTime()
        {
            return GetDriveTime() + GetFurnitureTime();
        }

        /// <summary>
        ///     Drive time in Minutes
        /// </summary>
        /// <param name="includeTravel">Indicates whether to include drive time from the Franchise location</param>
        /// <returns></returns>
        public decimal GetDriveTime(bool includeTravel = true)
        {
            IEnumerable<Address> stops = GetAddresses(includeTravel);
            decimal ret = 0m;
            Address lastAddress = null;
            foreach (Address stop in stops)
            {
                if (lastAddress != null)
                {
                    ret += lastAddress.GetTimeTo(stop);
                }

                lastAddress = stop;
            }

            return Math.Round(ret, 2);
        }

        /// <summary>
        ///     Get the maximum distance from the first billable stop to each other stop
        /// </summary>
        public decimal GetMoveRadius()
        {
            List<Address> addresses = GetAddresses(false).ToList();

            Address start = addresses.First();
            decimal radius = 0m;

            foreach (Address address in addresses.Except(new[] { start }))
            {
                decimal distance = start.GetDistanceTo(address);
                if (distance > radius)
                {
                    radius = distance;
                }
            }

            return radius;
        }

        /// <summary>
        ///     Get Total Move mileage, including to/from shop
        /// </summary>
        /// <returns></returns>
        public decimal GetTotalMileage()
        {
            return GetMileage(true);
        }

        /// <summary>
        ///     Get Mileage from Stop 1 to last stop
        /// </summary>
        /// <returns></returns>
        public decimal GetMoveMileage()
        {
            return GetMileage(false);
        }

        /// <summary>
        ///     Maximum minimum valuation limit = $.60/pound
        /// </summary>
        public decimal CalculateMinimumValuationLimit()
        {
            return GetWeight() * .60m;
        }

        public IEnumerable<Payment> GetPayments()
        {
            return QuotePayments.OrderBy(i => i.Date);
        }

        public IEnumerable<Payment> GetSuccessfulPayments()
        {
            return GetPayments().Where(i => i.Success && !i.IsCancelled);
        }

        public decimal GetTotalPayments()
        {
            return GetSuccessfulPayments().Sum(p => p.Amount);
        }
        public decimal GetTotalDue()
        {
            if (this.PricingType == QuotePricingType.Binding && this.GuaranteedPrice.HasValue)
            {
                return this.GuaranteedPrice.Value - this.GetTotalPayments();
            }
            else if (this.PricingType == QuotePricingType.Hourly && this.GuaranteedPrice.HasValue)
            {
               return (this.HourlyData.HourlyPrice * (this.HourlyData.CustomerTimeEstimate - 1) + this.HourlyData.FirstHourPrice) - this.GetTotalPayments();
            }
            else
                return 100000;
        }
        public decimal GetBalance()
        {
            if (!Postings.Any(p => p.IsComplete) || !FinalPostedPrice.HasValue)
            {
                return 0;
            }

            decimal tips = Postings.Sum(i => i.Posting_Employee_Rel.Sum(r => r.Tip));
            decimal final = FinalPostedPrice.Value + tips;
            return final - GetTotalPayments();
        }

        public Payment GetNewPayment()
        {
            var ret = new QuotePayment { QuoteID = QuoteID };
            
            return ret;
        }

        public void AddPayment(Payment p)
        {
            if (p is QuotePayment)
            {
                QuotePayments.Add((QuotePayment)p);
            }
            else
            {
                throw new InvalidOperationException("Can't add a non QuotePayment as payment on a quote");
            }
        }

        public bool CheckPayment(Payment p)
        {
            return QuotePayments.Any(quotepayment => quotepayment.TransactionID == p.TransactionID);
        }

        private decimal CalculateReplacementValuationCostForBinding(ReplacementValuation valuation)
        {
            if (valuation == null)
            {
                return 0;
            }

            if (valuation.Type != 2)
            {
                return valuation.Cost;
            }

            decimal weight = GetWeight();
            decimal cost = (((weight * 0.60m) * 1m) / 100m);
            return cost < 10 ? 10 : cost;
        }

        private decimal CalculateReplacementValuationCostForHourly(ReplacementValuation valuation)
        {
            if (valuation == null)
            {
                return 0;
            }

            var hourlymoveprice = HourlyData.FirstHourPrice + ((HourlyData.CustomerTimeEstimate - 1) * HourlyData.HourlyPrice);

            switch (valuation.Type)
            {
                case 1:
                {
                    return valuation.Cost;
                }
                case 2:
                {
                    return (hourlymoveprice * 5m) / 100m;
                }
                case 4:
                {
                    return (hourlymoveprice * 15m) / 100m;
                }
                default:
                {
                    return 0;
                }
            }
        }

        /// <summary>
        ///     Get a list of addresses in the order they will be visited during the move.
        /// </summary>
        /// <param name="includeTravel">Whether or not to include to/from the franchise location</param>
        /// <returns></returns>
        private IEnumerable<Address> GetAddresses(bool includeTravel)
        {
            List<Address> stops = GetStops().OrderBy(i => i.Sort).Select(i => i.Address).ToList();
            if (includeTravel)
            {
                stops.Insert(0, GetFranchiseAddress());
                stops.Add(GetFranchiseAddress());
            }

            return stops;
        }

        private decimal GetMileage(bool includeTravel)
        {
            IEnumerable<Address> stops = GetAddresses(includeTravel);
            decimal ret = 0m;
            Address lastAddress = null;
            foreach (Address stop in stops)
            {
                if (lastAddress != null)
                {
                    ret += lastAddress.GetDistanceTo(stop);
                }

                lastAddress = stop;
            }

            return Math.Round(ret, 2);
        }
    }
}