using Business.Interfaces;
using System;
using System.Text;
using Business.Repository.Models;

namespace Business.Models
{
    public partial class Payment
    {
        public Utility.PaymentTypes PaymentType
        {
            get
            {
                return (Utility.PaymentTypes)this.PaymentTypeID;
            }

            set
            {
                this.PaymentTypeID = (int)value;
            }
        }

        public Payment()
        {
        }

        public string GetNameDesc()
        {
            if (this is QuotePayment)
            {
                var qp = (QuotePayment)this;
                return "QUOTE " + qp.Quote.Lookup;
            }

            if (this is StoragePayment)
            {
                var sp = (StoragePayment)this;
                return "STORAGE " + sp.StorageWorkOrder.Lookup;
            }

            return String.Empty;
        }

        public aspnet_User GetProcessedBy()
        {
            if (this.AcceptedBy.HasValue)
            {
                var repo = new aspnet_UserRepository();
                return repo.Get(this.AcceptedBy.Value);
            }

            return default(aspnet_User);
        }

        public string GetMemoDesc()
        {
            var ret = String.Empty;

            if (this is QuotePayment)
            {
                var qp = this as QuotePayment;
                ret += "Quote " + qp.Quote.Lookup + " - " + qp.Quote.Account.DisplayName;
            }

            if (this is StoragePayment)
            {
                var sp = this as StoragePayment;
                ret += "Storage " + sp.StorageWorkOrder.Lookup + " - " + sp.StorageWorkOrder.Account.DisplayName;
            }

            if (this.Account_CreditCard != null)
            {
                ret += " - " + this.Account_CreditCard.DisplayCard() + " " + this.Account_CreditCard.DisplayExpiration();
            }

            return ret;
        }
    }
}
