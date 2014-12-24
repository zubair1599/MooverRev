using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Moovers.Tests.Repository
{
    public class TestPaymentRepository : Business.Interfaces.IPaymentRepository
    {
        private List<Business.Models.Payment> list;

        public TestPaymentRepository()
        {
            list = new List<Business.Models.Payment>();
        }

        public Business.Models.Payment GetByTransactionID(string transactionid)
        {
            return list.FirstOrDefault(i => i.TransactionID == transactionid);
        }

        public string GetUniqueTransactionID()
        {
            int transactionid = 0;
            while (this.GetByTransactionID(transactionid.ToString()) != null)
            {
                transactionid++;
            }

            return transactionid.ToString();
        }

        public Business.Models.Payment Get(Guid id)
        {
            return list.FirstOrDefault(i => i.PaymentID == id);
        }

        public void Add(Business.Models.Payment item)
        {
            list.Add(item);
        }

        public void Save()
        {
        }
    }
}
