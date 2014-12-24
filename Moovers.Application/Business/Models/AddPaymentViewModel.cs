using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Models;
using Business.Repository.Models;

namespace Business.ViewModels
{
    public class AddPaymentViewModel
    {
        public const string NewCardRadio = "[-NEW CARD-]";

        public Guid? quoteid { get; set; }

        public Guid? workOrderID { get; set; }

        public string method { get; set; }

        public decimal amount { get; set; }

        public string name { get; set; }

        public string cardnumber { get; set; }

        public string expirationmonth { get; set; }

        public string expirationyear { get; set; }

        public string cvv2 { get; set; }

        public string billingZip { get; set; }

        public string memo { get; set; }

        public string checkNumber { get; set; }

        public string personalCheckNumber { get; set; }

        public AccountSignature PaymentSignature { get; set; }

        public bool IsNewCard()
        {
            if (this.GetPaymentType() != Utility.PaymentTypes.CreditCard)
            {
                return false;
            }

            return (method == NewCardRadio);
        }

        public decimal GetTotalDue()
        {
            if (quoteid != null)
            {
            var repo = new Repository.QuoteRepository();
            
               var quote = repo.Get(quoteid.Value);
               return quote.GetTotalDue();
            }
            if (workOrderID != null)
            {
                var repo = new StorageWorkOrderRepository();

                var storage = repo.Get(workOrderID.Value);
                return storage.GetTotalDue();
            }
            return 0;
        }
        public Utility.PaymentTypes GetPaymentType()
        {
            if (method == "cash")
            {
                return Utility.PaymentTypes.Cash;
            }

            if (method == "cashiercheck")
            {
                return Utility.PaymentTypes.CashierCheck;
            }

            if (method == "personalcheck")
            {
                return Utility.PaymentTypes.PersonalCheck;
            }

            if (method == "other")
            {
                return Utility.PaymentTypes.Other;
            }
            
            return Utility.PaymentTypes.CreditCard;
        }
    }
}
