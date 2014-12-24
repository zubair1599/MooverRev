using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class CustomCrewCountRepository : RepositoryBase<CustomCrewCount>
    {
        private static readonly Func<MooversCRMEntities, int, int, int, Guid, CustomCrewCount> CompiledGetForDay = CompiledQuery.Compile(
            (MooversCRMEntities db, int day, int month, int year, Guid franchiseID) => (from i in db.CustomCrewCounts
                where i.Day == day
                      && i.Month == month
                      && i.Year == year
                select i).FirstOrDefault()
            );

        public override CustomCrewCount Get(Guid id)
        {
            return db.CustomCrewCounts.SingleOrDefault(i => i.CustomCrewID == id);
        }

        public void Remove(Guid id)
        {
            var crew = Get(id);
            db.CustomCrewCounts.DeleteObject(crew);
        }

        private int? GetForDay(int day, int month, int year, Guid franchiseID)
        {
            var cust = CompiledGetForDay(db, day, month, year, franchiseID);
            if (cust != null)
            {
                return cust.Count;
            }

            return null;
        }

        public IEnumerable<DateTime> GetAll(Guid franchiseid)
        {
            return (from c in db.CustomCrewCounts 
                where c.FranchiseID == franchiseid
                select new { 
                    c.Year, 
                    c.Month, 
                    c.Day 
                }).ToList().Select(i => new DateTime(i.Year, i.Month, i.Day));
        }

        public int? GetForDay(DateTime date, Guid franchiseID)
        {
            return GetForDay(date.Day, date.Month, date.Year, franchiseID);
        }
    }
}