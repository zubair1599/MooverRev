using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;
using Business.Utility;

namespace Business.Repository.Models
{
    public class PricingAlgorithmRepository : RepositoryBase<PricingAlgorithm>
    {

        internal PricingAlgorithmRepository(MooversCRMEntities entities)
        {
            this.db = entities;
        }

        public PricingAlgorithmRepository() { }

        /****
         * 
         * The following are all NCalc expressions http://ncalc.codeplex.com/
         * 
         * We needed a way to be able to easily change how pricing on quotes is calculated without changing all existing quotes.
         * The following algorithms are versioned in the database and associated with each quote.
         * 
         ***/
        private static readonly string DefaultItemLoad = @"
            if ({CustomTime} > 0, {CustomTime} * {Count} + ({ExtraTime} * {Count}), 
                if ({MoversRequired} = 1, ({CubicFeet} / {OneManCubicFeet}) * {Count} + ({ExtraTime} * {Count}),
                    if ({MoversRequired} = 2, ({CubicFeet} / {TwoManCubicFeet}) * {Count} + ({ExtraTime} * {Count}),
                        ({CubicFeet} / {ThreeManCubicFeet}) * {Count} + ({ExtraTime} * {Count})
                    )
                )
            )";

        private static readonly string DefaultItemLoadUnload = @"
            {LoadTime} + ({UnloadMultiplier} * {LoadTime})
        ";

        private static readonly string DefaultItemPrice = @"
            ({LoadUnloadTime} * {ServiceMinuteRate}) + {LiabilityCost}
        ";

        private static readonly string StopPrice = @"
            ((({BaseStopTime} * {MinimumMovers}) + {MoveTime}) * {ServiceMinuteRate}) + {FlatAdds}
        ";

        private static readonly string StopTime = @"
            {BaseMoveTime} * {ConditionMultiplier}
        ";

        private static readonly string QuoteDriveCost = @"
            ({ManDriveMinuteRate} * {Movers} * {DriveTime}) + ({DriveTime} * {Trucks} * {TruckDriveMinute})
        ";

        // > 600 mile algorithm uses a per truck cost
        private static readonly string LongDistanceAlgorithm = @"
            Ceiling(
                (({DriveMiles} * {DriverRate}) + {DriverFlatAdd}) / {LongDistancePercentage}
            ) * {Trucks}";

        private static readonly string QuoteTotalCost = @"
            if ({DriveMiles} > " + PricingAlgorithm.LongDistanceMoveMiles + ", " + LongDistanceAlgorithm + @",
            Ceiling(  ({DriveCost} + {LoadUnloadCost}) * {FlatMultiplier} ))
        ";

        private readonly Func<MooversCRMEntities, Guid, PricingAlgorithm> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, PricingAlgorithm>(
            (db, id) => db.PricingAlgorithms.SingleOrDefault(i => i.AlgorithmID == id)
            );

        public override PricingAlgorithm Get(Guid id)
        {
            // algorithms are extremely expensive to retreive from the DB, they're cached in the HttpRuntimeCache
            var cacheRepo = new CacheRepository();
            var key = "ALGORITHM" + id.ToString();
            if (cacheRepo.Get(key) == null)
            {
                var algorithm = CompiledGetByID(db, id);
                cacheRepo.Store(key, algorithm);
            }

            return cacheRepo.Get<PricingAlgorithm>(key);
        }

        /// <summary>
        /// Every Pricing Algorithm is versioned in the database, and the latest one is stored in text in this file.
        /// To update the Database algorithm, mark the "is current" bit, and a new "current" is added.
        /// </summary>
        private PricingAlgorithm GetCurrent(AlgorithmTypes type)
        {
            var stored = this.db.PricingAlgorithms.FirstOrDefault(i => i.AlgorithmType == (int)type && i.IsCurrent);
            if (stored != null)
            {
                return stored;
            }

            var algorithm = new PricingAlgorithm() {
                IsCurrent = true,
                AlgorithmType = (int)type,
                DateAdded = DateTime.Now,
                VariableList = String.Empty
            };

            if (type == AlgorithmTypes.ItemLoadTime)
            {
                algorithm.Text = DefaultItemLoad;
            }

            if (type == AlgorithmTypes.ItemLoadUnloadTime)
            {
                algorithm.Text = DefaultItemLoadUnload;
            }

            if (type == AlgorithmTypes.ItemPrice)
            {
                algorithm.Text = DefaultItemPrice;
            }

            if (type == AlgorithmTypes.StopPrice)
            {
                algorithm.Text = StopPrice;
            }

            if (type == AlgorithmTypes.StopTime)
            {
                algorithm.Text = StopTime;
            }

            if (type == AlgorithmTypes.DrivePrice)
            {
                algorithm.Text = QuoteDriveCost;
            }

            if (type == AlgorithmTypes.QuotePrice)
            {
                algorithm.Text = QuoteTotalCost;
            }

            this.Add(algorithm);
            db.SaveChanges();
            return algorithm;
        }

        public IEnumerable<AlgorithmItem> GetItemsFor(Guid quoteid)
        {
            var json = (from i in db.Quote_SavedItemList_Rel
                where i.QuoteID == quoteid
                select i.ItemList).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(json))
            {
                return Enumerable.Empty<AlgorithmItem>();
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<AlgorithmItem>>(json);
        }

        public Dictionary<string, object> GetConstantsFor(Guid quoteid, AlgorithmTypes type)
        {
            var json = (from i in db.Quote_PricingAlgorithm_Rel
                where i.QuoteID == quoteid
                      && i.PricingAlgorithm.AlgorithmType == (int)type
                select i.VariableList).FirstOrDefault();

            if (json == null)
            {
                return new Dictionary<string, object>();
            }

            var ret = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            
            // http://ncalc.codeplex.com/discussions/346702
            // There are some odd quirks when using both doubles and decimals in NCalc, so just make all of our constants decimals
            foreach (var i in ret.ToList())
            {
                if (i.Value is double)
                {
                    ret[i.Key] = (decimal)(double)i.Value;
                }
            }

            return ret;
        }

        private Func<MooversCRMEntities, Guid, Quote_SavedItemList_Rel> CompiledGetItemList = CompiledQuery.Compile<MooversCRMEntities, Guid, Quote_SavedItemList_Rel>(
            (db, quoteid) => db.Quote_SavedItemList_Rel.FirstOrDefault(i => i.QuoteID == quoteid)
            );

        /// <summary>
        /// Each quote saves a list of items that have been priced/weighted to the database, that way we can freely make changes to item weights without messing up every existing quote.
        /// </summary>
        public void UpdateSavedItemList(Guid quoteid, IEnumerable<AlgorithmItem> items)
        {
            var quote = db.Quotes.SingleOrDefault(i => i.QuoteID == quoteid);
            var savedRel = CompiledGetItemList(db, quoteid);
            if (savedRel == null)
            {
                savedRel = new Quote_SavedItemList_Rel {
                    QuoteID = quoteid
                };
                db.Quote_SavedItemList_Rel.AddObject(savedRel);
            }

            savedRel.ItemList = items.SerializeToJson();
            savedRel.Updated = DateTime.Now;

            if (quote != null)
            {
                var itemList = quote.GetItemList();
                foreach (var stop in quote.Stops)
                {
                    stop.UpdateCachedCost(itemList);
                }

                quote.UpdateCachedItemValues();
            }
        }

        private Func<MooversCRMEntities, Guid, AlgorithmTypes, Quote_PricingAlgorithm_Rel> CompiledGetByQuoteType = CompiledQuery.Compile<MooversCRMEntities, Guid, AlgorithmTypes, Quote_PricingAlgorithm_Rel>(
            (db, quoteid, type) => (from i in db.Quote_PricingAlgorithm_Rel
                where i.QuoteID == quoteid
                      && i.PricingAlgorithm.AlgorithmType == (int)type
                select i).FirstOrDefault()
            );

        public PricingAlgorithm GetForQuoteType(Guid quoteid, AlgorithmTypes type, Dictionary<string, object> variables)
        {
            var ret = CompiledGetByQuoteType(db, quoteid, type);
            if (ret == null)
            {
                var id = this.GetCurrent(type).AlgorithmID;
                ret = new Quote_PricingAlgorithm_Rel {
                    PricingAlgorithmID = id,
                    QuoteID = quoteid,
                    VariableList = variables.SerializeToJson()
                };
                db.Quote_PricingAlgorithm_Rel.AddObject(ret);
                this.Save();
            }

            return this.Get(ret.PricingAlgorithmID);
        }
    }
}