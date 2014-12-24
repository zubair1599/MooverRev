// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="StorageWorkOrder.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Enums;
    using Business.Interfaces;
    using Business.JsonObjects;
    using Business.Repository.Models;
    using Business.Utility;

    public partial class StorageWorkOrder_Quote_Rel
    {
        public StorageQuoteType StorageQuoteType
        {
            get { return (StorageQuoteType)StorageQuoteTypeID; }

            set { StorageQuoteTypeID = (int)value; }
        }
    }

    public sealed partial class StorageWorkOrder : IPayable
    {
        public StorageWorkOrder()
        {
            InvoicePeriod = InvoicePeriod.Monthly;
            NextInvoiceDate = DateTime.Today;
        }

        DateTime? IPayable.DateScheduled
        {
            get { return null; }
        }

        Guid IPayable.ID
        {
            get { return WorkOrderID; }
        }

        Franchise IPayable.Franchise
        {
            get { return new FranchiseRepository().GetStorage(); }
        }

        Guid IPayable.FranchiseID
        {
            get { return ((IPayable)this).Franchise.FranchiseID; }
        }

        private InvoicePeriod InvoicePeriod
        {
            get { return (InvoicePeriod)InvoicePeriodID; }
            set { InvoicePeriodID = (int)value; }
        }

        public void SetInventory(IEnumerable<ItemCollection> items)
        {
            foreach (ItemCollection item in items.Where(i => i.StorageCount > 0))
            {
                AddInventory(item.Item.ItemID, item.StorageCount);
            }
        }

        public void AddInventory(Guid itemid, int count)
        {
            var rel = new StorageWorkOrder_InventoryItem_Rel();
            StorageWorkOrder_InventoryItem_Rel.Add(rel);
            rel.InventoryItemID = itemid;
            rel.Count = count;
        }

        public IEnumerable<ItemCollection> GetInventory()
        {
            IEnumerable<IGrouping<Guid, StorageWorkOrder_InventoryItem_Rel>> inventory = StorageWorkOrder_InventoryItem_Rel.GroupBy(i => i.InventoryItemID);

            return
                inventory.Select(
                    item =>
                        new ItemCollection()
                        {
                            Count = item.Sum(i => i.Count),
                            IsBox = item.First().InventoryItem.IsBox,
                            Item = item.First().InventoryItem.ToJsonObject(),
                            Sort = item.First().InventoryItem.Sort,
                            StorageCount = item.Sum(i => i.Count),
                            AdditionalInfo = Enumerable.Empty<AdditionalInfo>()
                        });
        }

        public IEnumerable<StorageWorkOrder_InventoryItem_Rel> GetInventoryRels()
        {
            return StorageWorkOrder_InventoryItem_Rel.OrderBy(i => i.IsRemoved).ThenByDescending(i => i.IsOverstuffed).ThenBy(i => i.InventoryItem.Name);
        }

        public void AddQuote(Quote quote, StorageQuoteType type)
        {
            var rel = new StorageWorkOrder_Quote_Rel { QuoteID = quote.QuoteID, StorageQuoteTypeID = (int)type };
            StorageWorkOrder_Quote_Rel.Add(rel);
        }

        public IEnumerable<Quote> GetQuotes()
        {
            IEnumerable<Quote> quotes = StorageWorkOrder_Quote_Rel.Select(i => i.Quote);
            quotes =
                quotes.Where(i => i.StatusID != (int)QuoteStatus.Lost && i.StatusID != (int)QuoteStatus.Duplicate && i.StatusID != (int)QuoteStatus.Deferred);
            return quotes.OrderBy(i => (i.Schedules.FirstOrDefault() ?? new Schedule()).Date);
        }

        public IEnumerable<StorageInvoice> GetActiveInvoices()
        {
            return StorageInvoices.Where(i => !i.IsRemoved && !i.IsCancelled);
        }

        public IEnumerable<StorageInvoice> GetInvoices()
        {
            return StorageInvoices.OrderBy(i => i.ForDate).Where(i => !i.IsRemoved);
        }

        public void CreateInvoice()
        {
            if (MonthlyPayment == 0)
            {
                return;
            }

            if (!NextInvoiceDate.HasValue)
            {
                NextInvoiceDate = DateTime.Today;
            }

            var ret = new StorageInvoice { Amount = MonthlyPayment, Date = DateTime.Now, ForDate = NextInvoiceDate.Value, IsRemoved = false };
            StorageInvoices.Add(ret);

            if (InvoicePeriod == InvoicePeriod.Daily)
            {
                NextInvoiceDate = NextInvoiceDate.Value.Date.AddDays(1);
            }

            if (InvoicePeriod == InvoicePeriod.Weekly)
            {
                NextInvoiceDate = NextInvoiceDate.Value.Date.AddDays(7);
            }

            if (InvoicePeriod == InvoicePeriod.Monthly)
            {
                NextInvoiceDate = NextInvoiceDate.Value.Date.AddMonths(1);
            }

            if (InvoicePeriod == InvoicePeriod.Yearly)
            {
                NextInvoiceDate = NextInvoiceDate.Value.Date.AddYears(1);
            }

            // find any payments that have extra $$$
            var payments =
                GetPayments()
                    .Cast<StoragePayment>()
                    .Select(i => new { amount = i.Amount - i.GetInvoices().Sum(r => r.Amount ?? 0), payment = i })
                    .OrderBy(i => i.payment.Date)
                    .ToList();

            while (ret.GetBalance() > 0 && payments.Any())
            {
                var p = payments.First();
                decimal amount = Math.Min(p.amount, ret.GetBalance());
                if (amount > 0)
                {
                    ret.AddPayment(p.payment, amount);
                }

                payments.Remove(p);
            }
        }

        public IEnumerable<StorageVault> GetVaults()
        {
            return StorageWorkOrder_Vault_Rel.Where(i => !i.IsRemoved).Select(i => i.StorageVault).OrderWithPadding(i => i.Lookup, 10, false);
        }

        public IEnumerable<StorageWorkOrder_InventoryItem_Rel> GetOverstuffed()
        {
            return StorageWorkOrder_InventoryItem_Rel.Where(i => i.IsOverstuffed && !i.IsRemoved);
        }

        public StoragePayment GetLastSuccessfulPayment()
        {
            return StoragePayments.Where(i => !i.IsCancelled && i.Success).OrderByDescending(i => i.Date).FirstOrDefault();
        }

        /// <summary>
        ///     Gets the date their last unpaid invoice was due.
        /// </summary>
        /// <returns></returns>
        public DateTime? GetNextPayment()
        {
            if (GetActiveInvoices().Any(i => !i.IsPaid()))
            {
                return GetActiveInvoices().Where(i => !i.IsPaid()).Min(i => i.ForDate);
            }

            if (NextInvoiceDate.HasValue)
            {
                return NextInvoiceDate.Value;
            }

            return null;
        }

        /// <summary>
        ///     Get the number of days since the last unpaid invoice was due.
        /// </summary>
        /// <returns></returns>
        public int GetDaysOverdue()
        {
            DateTime? dueDate = GetNextPayment();
            if (!dueDate.HasValue || CancellationDate.HasValue || MonthlyPayment == 0)
            {
                return 0;
            }

            TimeSpan between = DateTime.Today - dueDate.Value;
            return (int)Math.Ceiling(between.TotalDays);
        }

        public object ToJsonObject()
        {
            return new { Balance = GetBalance(), Lookup = Lookup, NextPayment = NextInvoiceDate, DisplayName = Account.DisplayName };
        }

        public void Cancel(Guid userID)
        {
            NextInvoiceDate = null;

            // cancel future invoices when a storage account is cancelled...do not remove past invoices
            IEnumerable<StorageInvoice> futureInvoices = GetActiveInvoices().Where(i => !i.IsDue());
            foreach (StorageInvoice i in futureInvoices)
            {
                i.IsCancelled = true;
            }

            CancellationDate = DateTime.Now;
            CancelledBy = userID;
        }

        public decimal GetOverdueBalance()
        {
            IEnumerable<StorageInvoice> invoices = StorageInvoices.Where(i => i.IsDue() && !i.IsPaid() && !i.IsCancelled && !i.IsRemoved);
            return invoices.Sum(i => i.GetBalance());
        }

        public IEnumerable<File> GetFiles()
        {
            return Storage_File_Rel.Select(i => i.File).OrderBy(i => i.Created);
        }

        public IEnumerable<EmailLog> GetEmails()
        {
            return EmailLogs.OrderBy(i => i.DateSent);
        }

        public Storage_File_Rel AddFile(File f)
        {
            List<File> existing = GetFiles().ToList();
            int cur = 0;
            string origName = f.Name;
            while (existing.Any(i => i.Name == f.Name) && cur < 30)
            {
                cur++;
                f.Name = origName + " (" + cur.ToString() + ")";
            }

            return new Storage_File_Rel { FileID = f.FileID, StorageWorkOrder = this };
        }

        public IEnumerable<StorageComment> GetComments()
        {
            return StorageComments.OrderBy(i => i.Date);
        }

        public StorageStatement AddStatement(Guid? userid, string html)
        {
            var statement = new StorageStatement();
            var file = new File("Storage Statement - " + Lookup + " - " + DateTime.Now.ToShortDateString(), "application/pdf");
            file.HtmlContent = html;

            statement.File = file;
            statement.PrintedBy = userid;
            statement.Date = DateTime.Now;

            StorageStatements.Add(statement);
            return statement;
        }

        public decimal GetBalance()
        {
            return StorageInvoices.Where(i => !i.IsCancelled && !i.IsRemoved).Sum(i => i.Amount) - GetTotalPayments();
        }

        public IEnumerable<Payment> GetPayments()
        {
            return StoragePayments.OrderBy(i => i.Date);
        }

        public decimal GetTotalPayments()
        {
            return GetPayments().Where(i => !i.IsCancelled && i.Success).Sum(i => i.Amount);
        }

        public decimal GetTotalDue()
        {
            return GetBalance();
        }

        public Payment GetNewPayment()
        {
            var ret = new StoragePayment { WorkOrderID = WorkOrderID };
            return ret;
        }

        public void ResetAutomaticBilling()
        {
            HasPaymentError = false;
            LastPaymentError = String.Empty;
            FailedAutomaticBillingAttempts = 0;
            LastAutomaticBillingDate = null;
        }

        public void AddPayment(Payment p)
        {
            var storagePayment = p as StoragePayment;

            if (storagePayment == null)
            {
                return;
            }

            StoragePayments.Add(storagePayment);
            IOrderedEnumerable<StorageInvoice> invoices = GetActiveInvoices().Where(i => !i.IsPaid()).OrderBy(i => i.ForDate);

            var queue = new Queue<StorageInvoice>(invoices);
            decimal amount = p.Amount;
            while (amount > 0 && queue.Any())
            {
                StorageInvoice invoice = queue.Dequeue();
                decimal paymentAmount = Math.Min(invoice.GetBalance(), amount);
                invoice.AddPayment(storagePayment, paymentAmount);
                amount = amount - paymentAmount;
            }
        }

        public bool CheckPayment(Payment p)
        {
            return StoragePayments.Any(storepayment => storepayment.TransactionID == p.TransactionID);
        }

        public StorageComment AddComment(Guid aspUserID, string text)
        {
            var ret = new StorageComment();
            ret.UserID = aspUserID;
            ret.Text = text;
            ret.Date = DateTime.Now;
            StorageComments.Add(ret);
            return ret;
        }
    }
}