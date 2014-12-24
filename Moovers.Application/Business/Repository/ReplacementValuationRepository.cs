using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class ReplacementValuationRepository : RepositoryBase<ReplacementValuation>
    {
        private static readonly Func<MooversCRMEntities, Guid, ReplacementValuation> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, ReplacementValuation>(
            (db, id) => db.ReplacementValuations.SingleOrDefault(i => i.ValuationTypeID == id)
            );

        private static readonly Func<MooversCRMEntities, decimal, IEnumerable<ReplacementValuation>> CompiledGetForWeight = CompiledQuery.Compile<MooversCRMEntities, decimal, IEnumerable<ReplacementValuation>>(
            (db, weight) => (from r in db.ReplacementValuations
                where !r.MaximumValue.HasValue
                      || r.MaximumValue.Value == 100000
                      || ((r.PerPound * weight) <= r.MaximumValue.Value )
                orderby r.MaximumValue
                select r)
            );

        public override ReplacementValuation Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }
        public IEnumerable<ReplacementValuation> GetAll()
        {
            return db.ReplacementValuations;
        }
        public IEnumerable<ReplacementValuation> GetForWeight(decimal weight)
        {
            return CompiledGetForWeight(db, weight);
        }
    }
}