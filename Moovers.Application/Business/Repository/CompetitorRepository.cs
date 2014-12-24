using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class CompetitorRepository : RepositoryBase<Competitor>
    {
        public override Competitor Get(Guid id)
        {
            return db.Competitors.SingleOrDefault(i => i.CompetitorID == id);
        }

        public IEnumerable<Competitor> GetAll(Guid franchiseID)
        {
            return db.Competitors.Where(i => i.FranchiseID == franchiseID);
        }
    }
}