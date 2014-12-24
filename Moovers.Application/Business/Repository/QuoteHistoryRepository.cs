using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class QuoteHistoryRepository : RepositoryBase<QuoteHistory>
    {
        public override QuoteHistory Get(Guid id)
        {
            return db.QuoteHistories.SingleOrDefault(i => i.QuoteHistoryID == id);
        }

        public IEnumerable<QuoteHistory> GetByQuoteID(Guid quoteid)
        {
            return db.QuoteHistories.Where(i => i.QuoteID == quoteid);
        }
    }
}