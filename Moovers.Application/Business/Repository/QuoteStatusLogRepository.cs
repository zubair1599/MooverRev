using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class QuoteStatusLogRepository : RepositoryBase<QuoteStatusLog>
    {
        public override QuoteStatusLog Get(Guid id)
        {
            return db.QuoteStatusLogs.SingleOrDefault(i => i.StatusID == id);
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.QuoteStatusLogs.Where(i => i.UserID == userid);
            foreach (var item in items)
            {
                db.QuoteStatusLogs.DeleteObject(item);
            }
        }
    }
}