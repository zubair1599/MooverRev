using System;
using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Repository.Models;
using Business.ToClean.QuoteHelpers;
using NCalc;

namespace Business.Models
{
    /// <summary>
    /// All quote pricing is done through a utility called "NCalc", and each quote can have it's own unique pricing algorithm.
    /// 
    /// A list of Constants as well as all Item Weights/Sizes/Costs/Movers Required are saved in a database table, along w/ the pricing algorithm.
    /// </summary>
    public class AlgorithmItem
    {
        public Guid ItemID { get; set; }

        public decimal CubicFeet { get; set; }

        public decimal LiabilityCost { get; set; }

        public int MoversRequired { get; set; }

        public decimal CustomTime { get; set; }

        public string Name { get; set; }

        public string Room { get; set; }
    }

    public partial class Quote
    {
        /// <summary>
        /// Cost per minute of furniture unload/load time
        /// </summary>
        public const decimal ServiceMinuteRate = .92m;

        /// <summary>
        /// Cost per minute of drive time per man
        /// </summary>
        private const decimal ManDriveMinuteRate = .5m;

        /// <summary>
        /// Cost per minute of drive time per truck
        /// </summary>
        private const decimal TruckDriveMinute = 1.50m;

        /// <summary>
        /// Number of minutes to add for each stop
        /// </summary>
        public const decimal StopTime = 20m;

        private const decimal StorageVaultMonthlyFee = 50m;

        private const decimal StorageVaultCubicFeet = 175;

        private const decimal LongDistancePercentage = .1m;

        public const decimal LongDistanceDriverMileage = .25m;

        public const decimal LongDistanceDriverLoadUnload = 100m;

        public static readonly int MaxTruckWeight = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxTruckWeight"]);

        public static readonly int MaxTruckCubicFeet = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxTruckCubicFeet"]);

        public decimal CalculateDriveCost()
        {
            /*
             * Formula:({ManDriveMinuteRate} * {Movers} * {DriveTime}) + ({DriveTime} * {Trucks} * {TruckDriveMinute})
             */

            var func = this.GetPricingAlgorithm(AlgorithmTypes.DrivePrice);
            var variables = new Dictionary<string, object> {
                { "DriveTime", this.GetDriveTime() }, 
                { "Movers", this.GetMinimumMoversRequired() }, 
                { "Trucks", this.GetPricingTrucks() }
            };
            var test = func(variables);
            return func(variables);
        }

        public decimal CalculateGuaranteedPrice()
        {
            /***
             * Variable List :
             * 
             * 
             * Formula:
                if ({DriveMiles} > 660, 
                Ceiling(
                    (({DriveMiles} * {DriverRate}) + {DriverFlatAdd}) / {LongDistancePercentage}
                ) * {Trucks},
                Ceiling(  ({DriveCost} + {LoadUnloadCost}) * {FlatMultiplier} ))
             * 
             */

            var func = this.GetPricingAlgorithm(AlgorithmTypes.QuotePrice);
            var variables = new Dictionary<string, object> {
                { "DriveTime", this.GetDriveTime() }, 
                { "LoadUnloadCost", this.GetStops().Sum(i => i.GetCostEstimate()) }, 
                { "DriveCost", this.CalculateDriveCost() }, 
                { "DriveMiles", this.GetTotalMileage() }, 
                { "Trucks", this.GetPricingTrucks() }, 
                { "DriverRate", LongDistanceDriverMileage },
                { "DriverFlatAdd", LongDistanceDriverLoadUnload }
            };
            var test = func(variables);
            return func(variables);
        }

        public int GetMinimumMoversRequired()
        {
            if (!this.GetStops().Any())
            {
                return 2;
            }

            decimal driveTime = this.GetDriveTime();
            int byStop = this.GetStops().Max(i => i.GetMinimumMoversRequired());

            // on 30 minute drives, we can ignore the cubic foot thresholds for movers required
            if (driveTime < 30)
            {
                return byStop;
            }

            // on > 30 minute drives, minimum 2 men per truck
            int byTruck = GetMinimumTrucksRequired() * 2;
            return Math.Max(byStop, byTruck);
        }

        public int GetMinimumTrucksRequired()
        {
            decimal cubicFeet = this.GetCubicFeet();
            if (!this.GetStops().Any())
            {
                return 1;
            }

            if (cubicFeet == 0)
            {
                return 1;
            }

            var byFoot = (int)Math.Ceiling(cubicFeet / MaxTruckCubicFeet);
            return byFoot;
        }

        public int CalculateStorageVaults()
        {
            if (this.StorageVaults.HasValue)
            {
                return this.StorageVaults.Value;
            }

            var items = this.Stops.SelectMany(i => i.GetAllRoomsItems()).Select(i => i.GetStorageCubicFeet());
            var size = items.Sum();
            return (int)Math.Ceiling(size / Quote.StorageVaultCubicFeet);
        }

        public decimal CalculateMonthlyStorageCost()
        {
            return this.CalculateStorageVaults() * Quote.StorageVaultMonthlyFee;
        }

        /// <summary>
        /// Constants saved per quote that are saved, then passed into each pricing algorithm
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetConstants()
        {
            var ret = new Dictionary<string, object> {
                { "ServiceMinuteRate", Quote.ServiceMinuteRate }, 
                { "ManDriveMinuteRate", Quote.ManDriveMinuteRate }, 
                { "TruckDriveMinute", Quote.TruckDriveMinute }, 
                { "BaseStopTime", Quote.StopTime }, 
                { "StorageVaultMonthlyFee", Quote.StorageVaultMonthlyFee }, 
                { "StorageVaultCubicFeet", Quote.StorageVaultCubicFeet }, 
                { "MaxTruckWeight", Quote.MaxTruckWeight }, 
                { "MaxTruckCubicFeet", Quote.MaxTruckCubicFeet },
                { "OneManCubicFeet", Room_InventoryItem.OneManCubicFeet }, 
                { "TwoManCubicFeet", Room_InventoryItem.TwoManCubicFeet }, 
                { "ThreeManCubicFeet", Room_InventoryItem.ThreeManCubicFeet }, 
                { "LongDistancePercentage", Quote.LongDistancePercentage }, 
                { "FlatMultiplier", 1.12 }
            };
            return ret;
        }

        public IEnumerable<AlgorithmItem> GetItemList()
        {
            var repo = new PricingAlgorithmRepository();
            var savedItems = repo.GetItemsFor(this.QuoteID).ToList();
            var toKeep = savedItems.Select(i => i.ItemID);
            var thisItems = this.GetStops().SelectMany(s => s.GetAllRoomsItems()).Where(i => i.Count > 0).Select(i => new AlgorithmItem() {
                CubicFeet = i.InventoryItem.CubicFeet,
                ItemID = i.InventoryItemID,
                MoversRequired = i.InventoryItem.MoversRequired,
                LiabilityCost = i.InventoryItem.LiabilityCost ?? 0,
                CustomTime = i.InventoryItem.CustomTime ?? -1,
                Name = i.InventoryItem.Name,
                Room = i.Room.Name
            });

            var newItems = thisItems.Where(i => !toKeep.Contains(i.ItemID));
            return savedItems.Concat(newItems);
        }

        /// <summary>
        /// It's fairly common to access the same PricingAlgorithm several times in a single request
        /// Store the query results in this cache object.
        /// </summary>
        private List<Tuple<Guid, AlgorithmTypes, PricingAlgorithm, Dictionary<string, object>>> cache = new List<Tuple<Guid, AlgorithmTypes, PricingAlgorithm, Dictionary<string, object>>>();

        public Func<Dictionary<string, object>, decimal> GetPricingAlgorithm(AlgorithmTypes pricingType)
        {
            var repo = new PricingAlgorithmRepository();

            if (!cache.Any(i => i.Item1 == this.QuoteID && i.Item2 == pricingType))
            {
                var tmp = repo.GetForQuoteType(this.QuoteID, pricingType, this.GetConstants());
                var consts = repo.GetConstantsFor(this.QuoteID, pricingType);
                cache.Add(new Tuple<Guid, AlgorithmTypes, PricingAlgorithm, Dictionary<string, object>>(this.QuoteID, pricingType, tmp, consts));
            }

            var tuple = cache.FirstOrDefault(i => i.Item1 == this.QuoteID && i.Item2 == pricingType);
            var algorithm = tuple.Item3;
            var constants = tuple.Item4;
            var expr = new Expression(algorithm.Text);

            return (dict) => {
                foreach (var kvp in dict.Concat(constants))
                {
                    expr.Parameters[kvp.Key] = kvp.Value;
                }

                return decimal.Parse(expr.Evaluate().ToString());
            };
        }
    }

    public partial class Stop
    {
        public decimal GetCostEstimate()
        {
            if (this.CachedCostEstimate.HasValue)
            {
                return this.CachedCostEstimate.Value;
            }

            return EstimateCost(this.Quote.GetItemList());
        }

        public void UpdateCachedCost(IEnumerable<AlgorithmItem> savedItems)
        {
            this.CachedCostEstimate = this.EstimateCost(savedItems);
        }

        private decimal EstimateCost(IEnumerable<AlgorithmItem> savedItems)
        {
            /***
             * Variable List :
             * 
             * BaseStopTime - base time for any stop per mover
             * MoveTime -- time to move all furniture (accounting for furniture)
             * MinimumMovers - minimum movers required for quote // By stop or By Truck
             * Flat Adds - liability cost adds for items
             * ServiceMinuteRate -- cost of 1 man minute
             * 
             * Formula:((({BaseStopTime} * {MinimumMovers}) + {MoveTime}) * {ServiceMinuteRate}) + {FlatAdds}
             * 
             */
            var variables = new Dictionary<string, object> {
                { "MoveTime", this.GetMoveTime() }, 
                { "MinimumMovers", this.Quote.GetMinimumMoversRequired() }, 
                { "FlatAdds", this.GetAllRoomsItems().Sum(i => i.GetLiabilityCost(savedItems)) }
            };

            var algorithm = this.Quote.GetPricingAlgorithm(AlgorithmTypes.StopPrice);
            var test = algorithm(variables);
            test.ToString();
            return algorithm(variables);
        }

        public decimal EstimateCostNoConditions()
        {
            var flatAdds = this.GetAllRoomsItems().Sum(i => i.GetLiabilityCost(this.Quote.GetItemList()));
            var stopCost = Quote.StopTime * this.Quote.GetMinimumMoversRequired();
            return flatAdds + ((this.GetBaseMoveTime() + stopCost) * Quote.ServiceMinuteRate);
        }

        public IEnumerable<Room_InventoryItem> GetAllRoomsItems()
        {
            return this.GetRooms().SelectMany(i => i.Room_InventoryItems).OrderBy(i => i.Sort);
        }

        public decimal GetCubicFeet()
        {
            var rels = this.GetAllRoomsItems();
            return rels.Sum(i => i.GetCubicFeet());
        }

        public decimal GetWeight()
        {
            var rels = this.GetAllRoomsItems();
            return rels.Sum(i => i.GetWeight());
        }

        /// <summary>
        /// Estimated "Man Minutes" required
        /// </summary>
        /// <returns></returns>
        public decimal GetMoveTime()
        {
            /***
             * Variable List :
             * 
             * BaseMoveTime - time to move all items at the stop, no conditions
             * ConditionMultiplier - % increase for conditions
             * Service Minute - 
             * 
             * Formula: {BaseMoveTime} * {ConditionMultiplier}
             * 
             */
            var variables = new Dictionary<string, object> { 
                { "BaseMoveTime", this.GetBaseMoveTime() }, 
                { "ConditionMultiplier", this.GetConditionMultiplier() }
            };

            var algorithm = this.Quote.GetPricingAlgorithm(AlgorithmTypes.StopTime);
            var test = algorithm(variables);
            return algorithm(variables);
        }

        private decimal GetBaseMoveTime()
        {
            var quote = this.Quote;
            var itemList = this.Quote.GetItemList();
            return this.GetAllRoomsItems().Select(i => i.GetBaseMoveTime(quote, itemList)).Sum();
        }

        public decimal GetConditionMultiplier()
        {
            decimal multiplier = 1;

            // charge 10% more for each 50 foot of walk distance beyond the first 50 feet
            if (this.WalkDistance > 50)
            {
                multiplier += ((this.WalkDistance - 50) / 50) * .1m;
            }

            multiplier += (this.GetFlightCount() * .15m);
            multiplier += (this.GetStepCount() * .02m);

            if (this.ElevatorType == ElevatorType.Public)
            {
                multiplier += .3m;
            }

            if (this.ElevatorType == ElevatorType.Reserved)
            {
                multiplier += .2m;
            }

            return multiplier;
        }

        private int GetFlightCount()
        {
            var insideFlights = (this.InsideStairType == StairType.Flight) ? this.InsideStairsCount : 0;
            var outsideFlights = (this.OutsideStairType == StairType.Flight) ? this.OutsideStairsCount : 0;
            return insideFlights + outsideFlights;
        }

        private int GetStepCount()
        {
            var insideSteps = (this.InsideStairType == StairType.Step) ? this.InsideStairsCount : 0;
            var outsideSteps = (this.OutsideStairType == StairType.Step) ? this.OutsideStairsCount : 0;
            return insideSteps + outsideSteps;
        }

        public int GetMinimumMoversRequired()
        {
            int required = 2;
            var rooms = this.Rooms.Select(r => r.GetItems());
            var cubicFeet = this.GetCubicFeet();

            foreach (var r in rooms.Where(r => r.Any()))
            {
                required = Math.Max(r.Max(i => i.Item.MoversRequired), required);
            }

            if ((this.ElevatorType == ElevatorType.Public || this.ElevatorType == ElevatorType.Reserved) && cubicFeet >= 500)
            {
                required = Math.Max(required, 3);
            }

            if (this.GetFlightCount() >= 2 && cubicFeet >= 1000)
            {
                required = Math.Max(required, 3);
            }

            if (this.GetFlightCount() >= 3 && cubicFeet >= 500)
            {
                required = Math.Max(required, 3);
            }

            if (this.WalkDistance >= 250 && cubicFeet >= 500)
            {
                required = Math.Max(required, 3);
            }

            if (cubicFeet >= 1500)
            {
                required = Math.Max(required, 3);
            }

            if (cubicFeet >= 2250)
            {
                required = Math.Max(required, 4);
            }

            return required;
        }
    }

    public partial class Room_InventoryItem
    {
        /// <summary>
        /// On two man items, how many cu/ft 2 movers can load in a minute.
        /// </summary>
        public const decimal TwoManCubicFeet = (8 / 2);

        /// <summary>
        /// On one man items, how many cu/ft one mover can load in a minute
        /// </summary>
        public const decimal OneManCubicFeet = (2 / 1);

        /// <summary>
        /// On 3 man items, how many cu/ft three movers can load in a minute
        /// </summary>
        public const decimal ThreeManCubicFeet = (12 / 3);

        public decimal GetLiabilityCost(IEnumerable<AlgorithmItem> itemList)
        {
            var item = itemList.FirstOrDefault(i => i.ItemID == this.InventoryItemID);

            decimal cost = this.InventoryItem.LiabilityCost ?? 0;
            if (item != default(AlgorithmItem))
            {
                cost = item.LiabilityCost;
            }

            return cost * this.Count;
        }

        public decimal GetWeight()
        {
            var weight = this.InventoryItem.Weight;
            var additional = this.Room_InventoryItem_Option.Sum(i =>
                i.Option.HasValue ? i.InventoryItemQuestionOption.Weight ?? 0
                : i.InventoryItemQuestion.Weight ?? 0);
            return (weight + additional) * this.Count;
        }

        private decimal GetSingleItemCubicFeet()
        {
            var cubicfeet = this.InventoryItem.CubicFeet;
            var additional = this.Room_InventoryItem_Option.Sum(i => i.Option.HasValue ? i.InventoryItemQuestionOption.CubicFeet ?? 0
                                                                     : i.InventoryItemQuestion.CubicFeet ?? 0);
            return cubicfeet + additional;
        }

        public decimal GetCubicFeet()
        {
            return this.GetSingleItemCubicFeet() * this.Count;
        }

        public decimal GetStorageCubicFeet()
        {
            return this.GetSingleItemCubicFeet() * this.StorageCount;
        }

        public decimal GetCost(Quote q, IEnumerable<AlgorithmItem> itemList)
        {
            /***
             * Variable List :
             * 
             * LoadUnloadTime - time required to load this item
             * LiabilityCost - liability cost of item
             * ServiceMinuteRate - cost of a man service minute
             */
            var pricing = q.GetPricingAlgorithm(AlgorithmTypes.ItemPrice);
            var variables = new Dictionary<string, object> { 
                { "LoadUnloadTime", this.GetBaseMoveTime(q, itemList) }, 
                { "LiabilityCost", this.GetLiabilityCost(itemList) }
            };

            return pricing(variables);
        }

        private decimal GetLoadTime(IEnumerable<AlgorithmItem> savedItems)
        {
            var pricing = this.Room.Stop.Quote.GetPricingAlgorithm(AlgorithmTypes.ItemLoadTime);
            /**
             * Variable List :
             * 
             *  CustomTime -- inventory custom load time, -1 if null
             *  CubicFeet -- cubic feet of all items + all extra options
             *  Movers Required -- # of movers required for item
             *  Count -- # of total load items
             *  StorageCount - # of storage items
             *  ExtraTime - total time for "options" on a single item
             * 
             * Constants:
             * OneManCubicFeet  -- # of cubic feet per man minute one man loads
             * TwoManCubicFeet
             * ThreeManCubicFeet
             * 
             * Formula:
                if ({CustomTime} > 0, {CustomTime} * {Count} + ({ExtraTime} * {Count}), 
                    if ({MoversRequired} = 1, ({CubicFeet} / {OneManCubicFeet}) * {Count} + ({ExtraTime} * {Count}),
                        if ({MoversRequired} = 2, ({CubicFeet} / {TwoManCubicFeet}) * {Count} + ({ExtraTime} * {Count}),
                            ({CubicFeet} / {ThreeManCubicFeet}) * {Count} + ({ExtraTime} * {Count})
                        )
                    )
                )
             * 
             * Example:
             * 
             * 
             */
            var variables = new Dictionary<string, object>();

            // we save a list of items when the initial pricing is done
            var saved = savedItems.FirstOrDefault(i => i.ItemID == this.InventoryItemID);
            if (saved != default(AlgorithmItem))
            {
                variables.Add("CustomTime", saved.CustomTime);
                variables.Add("Count", this.Count);
                variables.Add("MoversRequired", saved.MoversRequired);
                variables.Add("CubicFeet", saved.CubicFeet + this.GetAddedCubicFeet());
                variables.Add("LiabilityCost", saved.LiabilityCost);
            }
            else {
                if (this.InventoryItem.CustomTime.HasValue)
                {
                    variables.Add("CustomTime", this.InventoryItem.CustomTime.Value);
                }
                else
                {
                    variables.Add("CustomTime", -1);
                }

                variables.Add("Count", this.Count);
                variables.Add("MoversRequired", this.InventoryItem.MoversRequired);
                variables.Add("CubicFeet", this.GetCubicFeet() + GetAddedCubicFeet());
            }

            variables.Add("ExtraTime", this.Room_InventoryItem_Option.Sum(i => i.Option.HasValue ? i.InventoryItemQuestionOption.Time ?? 0 : i.InventoryItemQuestion.Time ?? 0));
            var test = pricing(variables);
            return pricing(variables);
        }


        public decimal? GetAddedCubicFeet()
        {
            if (this.Room_InventoryItem_Option.Count > 0)
            {
                //Add first the main questions cubic feet

                decimal? extra = 0;
                extra = extra + this.Room_InventoryItem_Option.Where(o => o.InventoryItemQuestion!=null).Sum(o => o.InventoryItemQuestion.CubicFeet);
                // add sum questions
                extra = extra + this.Room_InventoryItem_Option.Where(o=>o.InventoryItemQuestionOption!=null).Sum(o =>  o.InventoryItemQuestionOption.CubicFeet);
                return extra;
            }
            return 0;
        }

        /// <summary>
        /// Number of minutes required to load and unload the item
        /// </summary>
        /// <returns></returns>
        public decimal GetBaseMoveTime(Quote q, IEnumerable<AlgorithmItem> itemList)
        {
            var algorithm  = q.GetPricingAlgorithm(AlgorithmTypes.ItemLoadUnloadTime);
            /**
             * Variable List :
             *  LoadTime -- item load time
             *  UnloadMultiplier -- % of time it takes to unload compared to unload (so .9 = 90%)
             * 
             * Formula: {LoadTime} + ({UnloadMultiplier} * {LoadTime})
             * 
             */
            var variables = new Dictionary<string, object> {
                { "LoadTime", this.GetLoadTime(itemList) }, 
                { "UnloadMultiplier", .9m }
            };
            var test = algorithm(variables);
            return algorithm(variables);
        }
    }
}
