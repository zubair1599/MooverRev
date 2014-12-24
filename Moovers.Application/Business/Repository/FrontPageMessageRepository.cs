using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class FrontPageMessageRepository : RepositoryBase<FrontPageMessage>
    {
        public override FrontPageMessage Get(Guid id)
        {
            return db.FrontPageMessages.SingleOrDefault(i => i.MessageID == id);
        }

        public void Remove(FrontPageMessage msg)
        {
            db.FrontPageMessages.DeleteObject(msg);
        }

        public IEnumerable<FrontPageMessage> GetLatest(int count = 15)
        {
            return (from m in db.FrontPageMessages
                orderby m.Date descending
                select m).Take(count);
        }
    }
}