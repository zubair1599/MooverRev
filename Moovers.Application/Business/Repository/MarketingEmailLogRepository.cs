using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class MarketingEmailLogRepository : RepositoryBase<MarketingEmailLog>
    {
        public override MarketingEmailLog Get(Guid id)
        {
            return db.MarketingEmailLogs.SingleOrDefault(i => i.LeadID == id);
        }
    }
}