using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class PaymentRepository : RepositoryBase<Payment>, Interfaces.IPaymentRepository
    {
        private static Func<MooversCRMEntities, Guid, Payment> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Payment>(
            (db, id) => db.Payments.SingleOrDefault(i => i.PaymentID == id)
            );

        private static Func<MooversCRMEntities, string, Payment> CompiledGetByTranID = CompiledQuery.Compile<MooversCRMEntities, string, Payment>(
            (db, transactionID) => db.Payments.SingleOrDefault(i => i.TransactionID == transactionID)
            );

        public override Payment Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public IEnumerable<Payment> GetForDay(Guid franchiseID, DateTime day)
        {
            var min = day.Date.AddTicks(-1);
            var max = day.Date.AddDays(1).AddTicks(-1);
            return (from p in db.Payments.Where(i => i.FranchiseID == franchiseID)
                where p.Date > min && p.Date < max
                select p);
        }

        public Payment GetByTransactionID(string transactionID)
        {
            return CompiledGetByTranID(db, transactionID);
        }

        /// <summary>
        /// Each payment is given a 8 digit transaction ID, starting with a "T"
        /// 
        /// These are passed to FirstData in the "Reference_Number" field
        /// </summary>
        public string GetUniqueTransactionID()
        {
            var chars = Enumerable.Range(0, 10).Select(i => i.ToString()[0]).ToList();
            var tranID = "T" + Utility.General.RandomString(7, chars);
            while (this.GetByTransactionID(tranID) != null)
            {
                tranID = "T" + Utility.General.RandomString(7, chars);
            }

            return tranID;
        }

        public IEnumerable<Payment> GetUndeposited()
        {
            return (from i in db.Payments
                where !i.IsDeposited
                      && i.PaymentTypeID != (int)Utility.PaymentTypes.CreditCard
                      && i.PaymentTypeID != (int)Utility.PaymentTypes.Other
                select i);
        }
    }
}