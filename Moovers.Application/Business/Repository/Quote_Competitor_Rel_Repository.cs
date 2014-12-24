using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class Quote_Competitor_Rel_Repository : RepositoryBase<Quote_Competitor_Rel>
    {
        public override Quote_Competitor_Rel Get(Guid id)
        {
            return db.Quote_Competitor_Rel.SingleOrDefault(i => i.RelID == id);
        }

        public void Remove(Quote_Competitor_Rel item)
        {
            db.Quote_Competitor_Rel.DeleteObject(item);
        }
    }
}