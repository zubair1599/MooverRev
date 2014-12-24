using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class StorageInvoice
    {
        public bool IsPaid()
        {
            return this.GetBalance() <= 0;
        }

        public bool IsDue()
        {
            return this.ForDate <= DateTime.Today;
        }

        public decimal GetBalance()
        {
            if (!this.GetPayments().Any())
            {
                return this.Amount;
            }

            return this.Amount - this.GetPayments().Sum(i => (decimal)i.Amount);
        }

        public IEnumerable<StorageInvoice_Payment_Rel> GetPayments()
        {
            return this.StorageInvoice_Payment_Rel.Where(i => i.StoragePayment.Success && !i.StoragePayment.IsCancelled);
        }

        public void AddPayment(StoragePayment p, decimal amount)
        {
            var rel = new StorageInvoice_Payment_Rel {
                StoragePayment = p,
                Amount = amount
            };
            this.StorageInvoice_Payment_Rel.Add(rel);
        }
    }
}
