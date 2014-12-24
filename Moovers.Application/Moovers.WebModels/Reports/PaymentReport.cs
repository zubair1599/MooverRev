using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Business.Models;

namespace Moovers.WebModels.Reports
{
    public class PaymentReport
    {
        public DateTime Day { get; set; }

        private IEnumerable<Payment> Payments { get; set; }

        public decimal GetCreditCardAmount()
        {
            return this.GetCreditCardPayments().Sum(i => i.Amount);
        }

        public decimal GetDepositAmount()
        {
            return this.GetForDeposit().Sum(i => i.Amount);
        }

        public decimal GetOtherAmount()
        {
            return this.GetOtherPayments().Sum(i => i.Amount);
        }

        public IEnumerable<Payment> GetForDeposit()
        {
            return this.Payments.Where(i => i.PaymentType == Business.Utility.PaymentTypes.Cash ||
                                            i.PaymentType == Business.Utility.PaymentTypes.CashierCheck ||
                                            i.PaymentType == Business.Utility.PaymentTypes.PersonalCheck);
        }

        public IEnumerable<Payment> GetOtherPayments()
        {
            return this.Payments.Where(i => i.PaymentType == Business.Utility.PaymentTypes.Other);
        }

        public IEnumerable<Payment> GetCreditCardPayments()
        {
            return this.Payments.Where(i => !i.IsCancelled && i.Success && i.PaymentType == Business.Utility.PaymentTypes.CreditCard);
        }

        public IEnumerable<Payment> GetFailedCancelledCreditCardPayments()
        {
            return this.Payments.Where(i => i.IsCancelled || !i.Success);
        }

        public PaymentReport(IEnumerable<Payment> payments, DateTime day)
        {
            this.Day = day;
            this.Payments = payments.ToList();
        }

        public HtmlString GetItemLink(HtmlHelper helper, Business.Models.Payment payment)
        {
            if (payment is Business.Models.QuotePayment)
            {
                var quotePayment = payment as Business.Models.QuotePayment;
                return helper.ActionLink("Quote " + quotePayment.Quote.Lookup, "Overview", new { Controller = "Quote", id = quotePayment.Quote.Lookup });
            }

            if (payment is Business.Models.StoragePayment) 
            {
                var storagePayment = payment as Business.Models.StoragePayment;
                return helper.ActionLink("Storage " + storagePayment.StorageWorkOrder.Lookup, "View", new { Controller = "Storage", id = storagePayment.StorageWorkOrder.Lookup });
            }

            return new HtmlString(String.Empty);
        }
    }
}
