using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class CrewRepository : RepositoryBase<Crew>
    {
        private readonly Func<MooversCRMEntities, DateTime, int, Guid, Crew> CompiledGetForDayLookup = CompiledQuery.Compile(
            (MooversCRMEntities db, DateTime date, int lookup, Guid franchiseid) =>
                (from crew in db.Crews
                    where crew.Day == date.Day
                          && crew.Month == date.Month
                          && crew.Year == date.Year
                          && crew.Lookup == lookup
                          && crew.FranchiseID == franchiseid
                    select crew).FirstOrDefault()
            );

        public override Crew Get(Guid id)
        {
            return db.Crews.SingleOrDefault(i => i.CrewID == id);
        }

        public Crew GetForDayLookup(DateTime date, int lookup, Guid franchiseID)
        {
            return CompiledGetForDayLookup(db, date, lookup, franchiseID);
        }
    }
}