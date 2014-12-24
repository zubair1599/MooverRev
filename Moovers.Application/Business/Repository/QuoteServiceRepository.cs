using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class QuoteServiceRepository : RepositoryBase<QuoteService>
    {
        public override QuoteService Get(Guid id)
        {
            return db.QuoteServices.SingleOrDefault(i => i.ServiceID == id);
        }

        public void Remove(QuoteService item)
        {
            db.QuoteServices.DeleteObject(item);
        }
    }
}