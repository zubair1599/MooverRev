using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class QuoteAccessLogRepository : RepositoryBase<QuoteAccessLog>
    {
        public override QuoteAccessLog Get(Guid id)
        {
            return db.QuoteAccessLogs.SingleOrDefault(i => i.AccessLogID == id);
        }

        public void Log(string action, Guid quoteid, Guid userid)
        {
            var log = new QuoteAccessLog()
            {
                Action = action,
                Date = DateTime.Now,
                QuoteID = quoteid,
                UserID = userid
            };

            this.Add(log);
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.QuoteAccessLogs.Where(i => i.UserID == userid);
            foreach (var item in items)
            {
                db.QuoteAccessLogs.DeleteObject(item);
            }
        }
    }
}