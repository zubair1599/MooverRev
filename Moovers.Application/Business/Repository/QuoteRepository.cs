// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="QuoteRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Objects;
    using System.Data.Objects.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;

    using Business.Enums;
    using Business.Interfaces;
    using Business.JsonObjects;
    using Business.Models;
    using Business.Repository.Models;
    using Business.ToClean.QuoteHelpers;
    using Business.Utility;

    using LinqKit;

    public class QuoteRepository : RepositoryBase<Quote>, IQuoteRepository
    {
        public const int MaxPriceDiscount = 1000000;

        public const int MaxHourlySourceTime = 400;

        public const int MaxHourlyTravelTime = 400;

        private static readonly int ExpiringDays = int.Parse(ConfigurationManager.AppSettings["ExpiringDays"]);

        private static readonly Expression<Func<Quote, decimal>> OrderByPrice =
            (q) =>
                q.PricingTypeID == (int)QuotePricingType.Hourly
                    ? ((q.CustomerTimeEstimate.Value - 1) * (q.HourlyPrice.Value)) + q.FirstHourPrice.Value
                    : q.GuaranteedPrice.Value;

        private static readonly Func<MooversCRMEntities, Guid, Quote> CompiledGetByID =
            CompiledQuery.Compile<MooversCRMEntities, Guid, Quote>((db, id) => db.Quotes.SingleOrDefault(i => i.QuoteID == id));

        private static readonly Func<MooversCRMEntities, string, Quote> CompiledGetByLookup =
            CompiledQuery.Compile<MooversCRMEntities, string, Quote>((db, id) => db.Quotes.SingleOrDefault(i => i.Lookup == id));

        public override void Add(Quote item)
        {
            if (item.ValuationTypeID == null)
            {
                item.ValuationTypeID = Guid.Parse("46637499-6B5B-4354-9C08-BC375FF64850");
                item.ReplacementValuationCost = 0.00m;
            }

            base.Add(item);
        }

        public IQueryable<Quote> GetSearch(Guid? franchiseID, SearchParser search, QuoteSort sort, bool desc)
        {
            // start with all "quotes" for a franchise, then loop through search tokens and filter

            Func<IQueryable<Quote>> getCollection = () => franchiseID.HasValue ? GetForFranchise(franchiseID.Value) : GetAll();
            IQueryable<Quote> quotes = getCollection();
            var lastToken = SearchTokenType.Query;
            foreach (SearchToken q in search.Tokens)
            {
                // for "and" and "or" searches, we just modify what is happening on the next "query" token
                if (q.TokenType == SearchTokenType.And || q.TokenType == SearchTokenType.Or)
                {
                    lastToken = q.TokenType;
                }
                else if (q.TokenType == SearchTokenType.Query)
                {
                    // for "AND" tokens, filter the current "quote" subset
                    if (lastToken == SearchTokenType.And || lastToken == SearchTokenType.Query)
                    {
                        quotes = GetSearchResults(quotes, q.Text);
                    }
                        // for "OR", start from all quotes for a franchise, and concat that w/ current search results
                    else if (lastToken == SearchTokenType.Or)
                    {
                        quotes = quotes.Concat(GetSearchResults(getCollection(), q.Text));
                    }

                    quotes = quotes.Distinct();
                }
            }

            quotes = OrderSearchResults(quotes, sort, desc);
            return quotes;
        }

        public QuoteStatList GetCumulativeStats(Guid? franchiseID, string query)
        {
            SearchParser parser = SearchParser.Parse(query);
            IQueryable<Quote> quotes = GetSearch(franchiseID, parser, QuoteSort.QuoteID, false);

            DateTime expiringDate = DateTime.Today.AddDays(ExpiringDays);
            DateTime thirtyDays = DateTime.Today.AddDays(-30);

            // hack -- "unassigned" is a special case, where we ignore the "User"
            List<string> users = db.aspnet_Users.Select(i => i.LoweredUserName).ToList();
            List<SearchToken> tokens = parser.Tokens.Where(t => !users.Contains(t.Text)).ToList();
            tokens.Add(new SearchToken() { Text = "AND", TokenType = SearchTokenType.And });
            tokens.Add(new SearchToken() { Text = "unassigned", TokenType = SearchTokenType.Query });
            var unassignedparser = new SearchParser() { Tokens = tokens };
            int unassigned = GetSearch(franchiseID, unassignedparser, QuoteSort.QuoteID, false).Count();
            // end hack

            IQueryable<Quote> open = quotes.Where(i => i.StatusID == (int)QuoteStatus.Open);
            IQueryable<Quote> scheduled = quotes.Where(i => i.StatusID == (int)QuoteStatus.Scheduled);
            IQueryable<Quote> expiring = open.Where(i => i.MoveDate <= expiringDate);

            IQueryable<Quote> lost = quotes.Where(i => i.StatusID == (int)QuoteStatus.Lost && i.CancellationDate.HasValue && i.CancellationDate > thirtyDays);
            IQueryable<Quote> won =
                quotes.Where(
                    i =>
                        i.StatusID == (int)QuoteStatus.Completed
                        && i.Schedules.Any(s => s.Date >= thirtyDays && !s.IsCancelled && s.Postings.Any(p => p.IsComplete)));

            IQueryable<Quote> deferred = quotes.Where(i => i.StatusID == (int)QuoteStatus.Deferred);
            return new QuoteStatList()
            {
                OpenCount = open.Count(),
                OpenAmount = open.Any() ? open.Select(Quote.GetCostExpr).Sum() : 0m,
                ScheduledCount = scheduled.Count(),
                ScheduledAmount = scheduled.Any() ? scheduled.Select(Quote.GetCostExpr).Sum() : 0m,
                UnassignedCount = unassigned,
                ExpiringCount = expiring.Count(),
                Lost30DaysCount = lost.Count(),
                Lost30DaysAmount = lost.Any() ? lost.Select(Quote.GetCostExpr).Sum() : 0m,
                DeferredCount = deferred.Count(),
                DeferredAmount = deferred.Any() ? deferred.Select(Quote.GetCostExpr).Sum() : 0m,
                Won30DaysCount = won.Count(),
                Won30DaysAmount = won.Any() ? won.Select(Quote.GetCostExpr).Sum() : 0m
            };
        }

        public IEnumerable<Quote> GetLastAccessed(Guid userid)
        {
            IQueryable<Guid> quoteids =
                (from log in db.QuoteAccessLogs where log.UserID == userid orderby log.Date descending select log).Take(20)
                    .Select(i => i.QuoteID)
                    .Distinct()
                    .Take(5);

            return quoteids.Select(q => db.Quotes.FirstOrDefault(i => i.QuoteID == q));
        }

        public override Quote Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public IQueryable<Quote> GetAll()
        {
            return db.Quotes;
        }

        public IQueryable<Quote> GetForFranchise(Guid franchiseID)
        {
            return db.Quotes.Where(i => i.FranchiseID == franchiseID);
        }

        public Quote Get(string lookup)
        {
            return CompiledGetByLookup(db, lookup);
        }

        public IQueryable<Quote> GetWithValuation(Guid franchiseID, DateTime start, DateTime end)
        {
            return GetWonBetween(franchiseID, start, end).Where(i => (i.ReplacementValuationCost.HasValue && i.ReplacementValuationCost.Value > 0));
        }

        public IQueryable<Quote> GetBookedBetween(Guid? franchiseID, DateTime start, DateTime end)
        {
            return (from i in db.Quotes.Include("Schedules").Include("Schedules.Postings")
                where (!franchiseID.HasValue || i.FranchiseID == franchiseID) && i.Schedules.Any(s => !s.IsCancelled && s.Date >= start && s.Date <= end)
                select i);
        }

        public IQueryable<Quote> GetUnscheduled()
        {
            return (from i in db.Quotes where !i.Schedules.Any() && i.StatusID == (int)QuoteStatus.Open select i);
        }

        public IQueryable<Quote> GetQuotedBetween(Guid? franchiseID, DateTime start, DateTime end)
        {
            IQueryable<Quote> collection = (franchiseID.HasValue) ? GetForFranchise(franchiseID.Value) : GetAll();

            return (from i in collection where i.Schedules.All(s => s.IsCancelled) && i.Created <= start && i.Created >= end select i);
        }

        public IQueryable<Quote> GetOpenForUser(Guid userid)
        {
            return (from i in db.Quotes where i.AccountManagerID == userid && i.StatusID == (int)QuoteStatus.Open select i);
        }

        public IQueryable<Quote> GetOpenForUser(Guid franchiseID, Guid userid)
        {
            return GetOpenForUser(userid).Where(i => i.FranchiseID == franchiseID);
        }

        public IQueryable<Quote> GetForAccount(Guid accountid)
        {
            return
                (from i in db.Quotes where (i.AccountID == accountid || i.ShippingAccountID == accountid) && i.StatusID != (int)QuoteStatus.Duplicate select i);
        }

        public IQueryable<Quote> GetActiveOn(DateTime day)
        {
            DateTime minday = day.Date.AddTicks(-1);
            DateTime maxday = day.AddDays(1).AddTicks(-1);
            return (from i in GetAll()
                let schedules = i.Schedules
                where
                    (i.Created > minday && i.Created < maxday)
                    || (i.DateScheduled.HasValue && i.DateScheduled.Value > minday && i.DateScheduled.Value < maxday
                        && !schedules.Any(s => s.IsCancelled && (s.DateCreated < minday || s.DateCreated > maxday)))
                    || (i.DateModified > minday && i.DateModified < maxday && i.StatusID == (int)QuoteStatus.Lost)
                select i);
        }

        public IQueryable<Quote> GetWonBetween(Guid? franchiseID, DateTime start, DateTime end)
        {
            IQueryable<Quote> collection = (franchiseID.HasValue) ? GetForFranchise(franchiseID.Value) : GetAll();
            return (from i in collection
                where
                    i.Schedules.Any(s => s.Date >= start && s.Date <= end)
                    && i.Schedules.All(s => !s.IsCancelled && s.Postings.Any() && s.Postings.All(p => p.IsComplete))
                select i);
        }

        public Quote UpdateSavedItemList(Quote quote)
        {
            var algorithmRepo = new PricingAlgorithmRepository(db);
            algorithmRepo.UpdateSavedItemList(quote.QuoteID, quote.GetItemList());
            return quote;
        }

        public void UpdateGuaranteedPrice(Guid quoteid, decimal? adjustment = null, bool force = false)
        {
            Quote quote = Get(quoteid);
            quote = UpdateSavedItemList(quote);

            if (quote.GetStops().Any())
            {
                if (!quote.ForceFranchiseID.HasValue)
                {
                    // gets the stop by which a quote should be assigned to a franchise
                    Stop stop = quote.GetAssignmentStop();
                    var franchiseRepo = new FranchiseRepository();
                    ZipCode zip = stop.Address.GetZip();
                    Franchise franchise = franchiseRepo.GetAssignedTo(zip);
                    quote.FranchiseID = franchise.FranchiseID;
                }
                else
                {
                    quote.FranchiseID = quote.ForceFranchiseID.Value;
                }

                // A cached version of "move radius" so we can quickly determine whether a move is Local/National/Regional
                quote.CachedOneWayMoveMiles = quote.GetMoveRadius();
            }

            if (quote.PricingType != QuotePricingType.Binding && !force)
            {
                return;
            }
           
            
            if (quote.DiscountPriority.Equals((int)DiscountType.DiscountByValue) || quote.DiscountPriority==null)
            {
                decimal adjustments = 0, basePrice = 0, guaranteedPrice = 0;
                basePrice = quote.CalculateGuaranteedPrice();
                adjustments = adjustment ?? ((quote.PricingType == QuotePricingType.Binding) ? quote.GuaranteeData.Adjustments : 0m);
                
                guaranteedPrice = basePrice + adjustments;

                if (basePrice == 0 || (adjustments / basePrice * 100) > MaxPriceDiscount)
                {
                    adjustments = Math.Floor(basePrice * MaxPriceDiscount) / 100;
                }
                quote.GuaranteeData = new GuaranteedInfo() { BasePrice = basePrice, GuaranteedPrice = guaranteedPrice, Adjustments = adjustments };
            }
            else if (quote.DiscountPriority.Equals((int)DiscountType.DiscountbyPercentage))
            {
                decimal adjustments = 0, basePrice = 0, guaranteedPrice = 0;
                basePrice = quote.CalculateGuaranteedPrice();
                var discountValue = (basePrice / 100) * (decimal)quote.AdjustmentPercentage;
                guaranteedPrice = basePrice + discountValue;
                adjustment = discountValue;
                quote.GuaranteeData = new GuaranteedInfo() { BasePrice = basePrice, GuaranteedPrice = guaranteedPrice, Adjustments = adjustments };    

            }
            

            
        }

        public IQueryable<QuoteMapDirection> GetMooversDirections(DateTime day)
        {
            IQueryable<QuoteMapDirection> list =
                db.QuoteMapDirections.Where(
                    q => q.CheckinTime.Value.Day == day.Day && q.CheckinTime.Value.Month == day.Month && q.CheckinTime.Value.Year == day.Year);
            return list;
        }

        /// <summary>
        ///     Executes a search on a single word, filtering a larger collection based on that word.
        /// </summary>
        private IQueryable<Quote> GetSearchResults(IQueryable<Quote> baseCollection, string search)
        {
            IQueryable<Quote> quotes = db.Quotes.Where(i => false);

            // if empty search, just return open quotes
            if (String.IsNullOrEmpty(search))
            {
                return (from q in baseCollection where (q.StatusID != (int)QuoteStatus.Duplicate) orderby q.MoveDate select q);
            }

            IDictionary<string, string> statuses = QuoteStatus.Open.ToDictionary();
            Tuple<DateTime, DateTime> dateSearch = Date.GetDatesFromSearch(search);
            Guid? userSearch = General.GetAspUserFromSearch(search);

            if (dateSearch != null)
            {
                DateTime startSearch = dateSearch.Item1;
                DateTime endSearch = dateSearch.Item2;
                quotes = quotes.Concat(
                    from q in baseCollection
                    where // where scheduled and completed in the range
                        (q.StatusID == (int)QuoteStatus.Completed
                         && q.Schedules.Any(i => i.Date >= startSearch && i.Date <= endSearch && !i.IsCancelled && i.Postings.Any(p => p.IsComplete)))
                        || // or scheduled in the range
                        (q.StatusID != (int)QuoteStatus.Completed && q.Schedules.Any(i => i.Date >= startSearch && i.Date <= endSearch && !i.IsCancelled))
                        || // or, cancelled and was last acted on in the range
                        (q.StatusID == (int)QuoteStatus.Lost && q.CancellationDate.HasValue && q.CancellationDate >= startSearch
                         && q.CancellationDate <= endSearch) // or, open/imported and expected move date in the range
                        || ((q.AccountManager.LoweredUserName == "mpimport" || q.StatusID == (int)QuoteStatus.Open) && q.MoveDate >= startSearch
                            && q.MoveDate <= endSearch)
                    select q);
            }
            else if (baseCollection.Any(i => i.Lookup == search))
            {
                quotes = quotes.Concat(from q in baseCollection where q.Lookup == search select q);
            }
            else if (search == "survey")
            {
                quotes = quotes.Concat(from q in baseCollection where q.QuoteSurveys.Any(s => !s.IsCancelled) select q);
            }
            else if (search == "expiring" || search == "expired")
            {
                DateTime date = DateTime.Today.AddDays(ExpiringDays);
                quotes = quotes.Concat(from q in baseCollection where (q.StatusID == (int)QuoteStatus.Open) && q.MoveDate <= date select q);
            }
            else if (Enum.GetNames(typeof(QuoteType)).Any(i => i == search))
            {
                var quotetype = (QuoteType)Enum.Parse(typeof(QuoteType), search);

                /*
                 * Local = 0-100 miles
                 * Regional = 100-500 miles
                 * National = 500+ miles
                 */
                var min = (int)quotetype;
                int max = Int32.MaxValue;
                if (quotetype == QuoteType.Local)
                {
                    max = (int)QuoteType.Regional;
                }

                if (quotetype == QuoteType.Regional)
                {
                    max = (int)QuoteType.National;
                }

                quotes = quotes.Concat(from q in baseCollection where q.CachedOneWayMoveMiles >= min && q.CachedOneWayMoveMiles <= max select q);
            }
            else if (statuses.Any(i => i.Value.ToLower() == search.ToLower()))
            {
                foreach (KeyValuePair<string, string> kvp in statuses)
                {
                    if (search.ToLower() == kvp.Value.ToLower())
                    {
                        var status = (QuoteStatus)Enum.Parse(typeof(QuoteStatus), kvp.Key);

                        quotes =
                            baseCollection.Where(
                                i =>
                                    i.StatusID == (int)status
                                        // there was a bug where "WON" moves were marked as cancelled - the cancelled status doesn't exist elsewhere
                                    || (status == QuoteStatus.Completed && i.StatusID == (int)QuoteStatus.Cancelled));

                        if (status == QuoteStatus.Cancelled)
                        {
                            quotes = baseCollection.Where(i => i.Schedules.Any() && i.Schedules.All(s => s.IsCancelled));
                        }
                    }
                }
            }
            else if (userSearch.HasValue)
            {
                quotes =
                    quotes.Concat((from q in baseCollection where q.AccountManagerID == userSearch.Value select q))
                        .Where(i => i.StatusID != (int)QuoteStatus.Duplicate);

                // for unassigned specifically, only makes sense for "open" quotes.
                if (search == "unassigned")
                {
                    quotes = quotes.Where(i => i.StatusID == (int)QuoteStatus.Open);
                }
            }
            else if (
                db.Accounts.OfType<PersonAccount>()
                    .Any(
                        i =>
                            i.FirstName.StartsWith(search.ToLower()) || i.LastName.StartsWith(search.ToLower())
                            || (i.FirstName + " " + i.LastName).StartsWith(search.ToLower())))
            {
                List<Guid> accts =
                    db.Accounts.OfType<PersonAccount>()
                        .Where(
                            i =>
                                i.FirstName.StartsWith(search.ToLower()) || i.LastName.StartsWith(search.ToLower())
                                || (i.FirstName + " " + i.LastName).StartsWith(search.ToLower()))
                        .Select(i => i.AccountID)
                        .ToList();
                quotes = quotes.Concat((from q in baseCollection where accts.Contains(q.AccountID) select q));
            }
            else
            {
                // if searched by an account id, get quotes associated with that
                var accountRepo = new AccountRepository(db);
                Account account = accountRepo.Get(search);
                if (account != null)
                {
                    quotes =
                        quotes.Concat(from q in baseCollection where q.AccountID == account.AccountID || q.ShippingAccountID == account.AccountID select q);
                }

                //var matchingAccounts = accountRepo.SearchNames(franchiseID, search).Select(i => i.account);
                //var acctQuotes = baseCollection.Where(i => matchingAccounts.Contains(i.ShippingAccount) || matchingAccounts.Contains(i.Account));
                //quotes = quotes.Concat(acctQuotes);
            }

            return quotes.Where(i => i.StatusID != (int)QuoteStatus.Duplicate);
        }

        private IQueryable<Quote> OrderSearchResults(IQueryable<Quote> quotes, QuoteSort sort, bool desc)
        {
            if (sort == QuoteSort.QuoteID)
            {
                quotes = quotes.OrderWithPadding(i => i.Lookup, 12, desc);
            }

            if (sort == QuoteSort.Account)
            {
                quotes =
                    quotes.Select(i => new { person = i.Account as PersonAccount, business = i.Account as BusinessAccount, quote = i })
                        .OrderWithDirection(
                            i => i.person != null ? i.person.FirstName + " " + i.person.LastName : i.business != null ? i.business.Name : String.Empty,
                            desc)
                        .Select(i => i.quote);
            }

            if (sort == QuoteSort.Date || sort == QuoteSort.DaysToMove)
            {
                // expression to get the expected move date
                // if the move is scheduled, get the date of the scheduled move
                // if the move isn't scheduled, get the "expected" move date
                Expression<Func<Quote, DateTime>> getMoveDate =
                    (i => i.Schedules.Any(s => !s.IsCancelled) ? i.Schedules.Where(s => !s.IsCancelled).Min(s => s.Date) : i.MoveDate);

                // expression to get whether or not a quote has been "completed"
                Expression<Func<Quote, bool>> getIsCompleted =
                    (i => i.StatusID == (int)QuoteStatus.Completed || i.StatusID == (int)QuoteStatus.Lost || i.StatusID == (int)QuoteStatus.Cancelled);

                // DaysToMove is just a special case of "Move Date", where Completed and Lost moves are always sorted at the end
                // this is the "Date" we use to sort completed moves at the end
                DateTime completedSortDate = desc ? DateTime.MinValue : DateTime.MaxValue;

                quotes =
                    quotes.AsExpandable()
                        .OrderWithDirection(i => (sort == QuoteSort.DaysToMove && getIsCompleted.Invoke(i)) ? completedSortDate : getMoveDate.Invoke(i), desc);
            }

            if (sort == QuoteSort.Price)
            {
                quotes = quotes.OrderWithDirection(OrderByPrice, desc);
            }

            if (sort == QuoteSort.Type)
            {
                quotes =
                    quotes.OrderWithDirection(
                        i =>
                            i.CachedOneWayMoveMiles.HasValue
                                ? i.CachedOneWayMoveMiles.Value < (int)QuoteType.Regional
                                    ? "Local"
                                    : i.CachedOneWayMoveMiles.Value < (int)QuoteType.National ? "Regional" : "National"
                                : "",
                        desc);
            }

            if (sort == QuoteSort.SalesID)
            {
                quotes = quotes.OrderWithDirection(i => i.AccountManager.LoweredUserName, desc);
            }

            if (sort == QuoteSort.LastModified)
            {
                string pad = "0000000000";
                quotes =
                    quotes.OrderWithDirection(i => i.DateModified, desc)
                        .ThenWithDirection(i => SqlFunctions.Stuff(pad, pad.Length - i.Lookup.Length + 1, i.Lookup.Length, i.Lookup), desc);
            }

            if (sort == QuoteSort.VisualSurvey)
            {
                quotes = quotes.OrderWithDirection(i => i.QuoteSurveys.Any(s => !s.IsCancelled), desc);
            }

            if (sort == QuoteSort.Status)
            {
                quotes = quotes.OrderWithDirection(i => i.StatusID, desc);
            }

            return quotes;
        }
    }
}