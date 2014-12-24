using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moovers.Tests.Repository
{
    public class TestTransactionLogRepository : Business.Interfaces.IRepository<Business.Models.TransactionLog>
    {
        private List<Business.Models.TransactionLog> list;

        public TestTransactionLogRepository()
        {
            list = new List<Business.Models.TransactionLog>();
        }

        public IEnumerable<Business.Models.TransactionLog> GetAll()
        {
            return list;
        }

        public Business.Models.TransactionLog Get(Guid id)
        {
            return list.FirstOrDefault(i => i.TransactionLogID == id);
        }

        public void Add(Business.Models.TransactionLog item)
        {
            list.Add(item);
        }

        public void Save()
        {
        }
    }
}
