using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Utility;
using Business.ViewModels;

namespace Business.Repository.Models
{
    public class StorageWorkOrderRepository : RepositoryBase<StorageWorkOrder>
    {
        private const int StorageInvoiceDaysAhead = 20;

        private static readonly Func<MooversCRMEntities, Guid, StorageWorkOrder> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, StorageWorkOrder>(
            (db, id) => db.StorageWorkOrders.SingleOrDefault(i => i.WorkOrderID == id)
            );

        private static readonly Func<MooversCRMEntities, string, StorageWorkOrder> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, StorageWorkOrder>(
            (db, id) => db.StorageWorkOrders.SingleOrDefault(i => i.Lookup == id)
            );

        private static readonly Func<MooversCRMEntities, bool, IQueryable<StorageWorkOrder>> CompiledGetByActive = CompiledQuery.Compile<MooversCRMEntities, bool, IQueryable<StorageWorkOrder>>(
            (db, active) => db.StorageWorkOrders.Include("Account").Include("StorageInvoices").Include("StorageInvoices.StorageInvoice_Payment_Rel").Include("StoragePayments").Include("StorageWorkOrder_InventoryItem_Rel").Where(i => i.CancellationDate.HasValue == !active)
            );

        private static readonly Func<MooversCRMEntities, DateTime, IQueryable<StorageInvoice>> CompiledGetDue = CompiledQuery.Compile<MooversCRMEntities, DateTime, IQueryable<StorageInvoice>>(
            (db, date) => (from i in db.StorageInvoices
                let payments = i.StorageInvoice_Payment_Rel.Select(r => r.StoragePayment).Where(p => !p.IsCancelled && p.Success)
                where
                    !i.IsCancelled && !i.IsRemoved
                    &&
                    (i.ForDate <= date &&
                     (!payments.Any() && i.Amount > 0) || payments.Sum(p => p.Amount) < i.Amount
                        )
                select i)
            );

        private static IOrderedQueryable<StorageWorkOrder> SortWorkOrders(IQueryable<StorageWorkOrder> items, StorageSort sort, bool desc)
        {
            if (sort == StorageSort.Lookup)
            {
                return items.OrderWithPadding(i => i.Lookup, 10, desc);
            }

            if (sort == StorageSort.Account)
            {
                return (IOrderedQueryable<StorageWorkOrder>)
                    (from i in items
                        let personaccount = i.Account as PersonAccount
                        let businessaccount = i.Account as BusinessAccount
                        select new {
                            item = i,
                            person = personaccount,
                            business = businessaccount
                        }).OrderWithDirection(i => i.business != null ? i.business.Name : i.person.LastName + ", " + i.person.FirstName, desc).Select(i => i.item);
            }

            if (sort == StorageSort.LastPayment)
            {
                return items.ToList().AsQueryable().OrderWithDirection(i => (i.GetLastSuccessfulPayment() ?? new StoragePayment()).Date, desc);
            }

            if (sort == StorageSort.MonthlyPayment)
            {
                return items.OrderWithDirection(i => i.MonthlyPayment, desc);
            }

            if (sort == StorageSort.Balance)
            {
                return items.ToList().AsQueryable().OrderWithDirection(i => i.GetBalance(), desc);
            }

            if (sort == StorageSort.OverdueBalance)
            {
                return items.ToList().AsQueryable().OrderWithDirection(i => i.GetOverdueBalance(), desc);
            }

            if (sort == StorageSort.NextPayment)
            {
                return items.ToList().AsQueryable().OrderWithDirection(i => i.GetNextPayment(), desc);
            }

            if (sort == StorageSort.DaysOverdue)
            {
                // sort by "days overdue" is the same as "next payment", with two exceptions
                // Storage without a monthly payment is never overdue, and overdue is the inverse of "next payment"
                return items.ToList().AsQueryable().OrderWithDirection(i => i.MonthlyPayment == 0 ? DateTime.Today.AddYears(1) : i.GetNextPayment(), !desc);
            }

            if (sort == StorageSort.Vaults)
            {
                return items.OrderWithDirection(i => i.StorageWorkOrder_Vault_Rel.Count(r => !r.IsRemoved), desc);
            }

            if (sort == StorageSort.Oversize)
            {
                return items.OrderWithDirection(i => i.StorageWorkOrder_InventoryItem_Rel.Count(r => r.IsOverstuffed && !r.IsRemoved), desc);
            }

            if (sort == StorageSort.AutoBill)
            {
                return items.OrderWithDirection(i => i.IsAutomaticBilling, desc);
            }

            throw new InvalidOperationException();
        }

        public IEnumerable<StorageWorkOrder> GetForAccount(Guid accountid)
        {
            return db.StorageWorkOrders.Where(i => i.AccountID == accountid);
        }

        public override StorageWorkOrder Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public StorageWorkOrder Get(string lookup)
        {
            return CompiledGetByLookup(db, lookup);
        }

        public IQueryable<StorageWorkOrder> GetActive(StorageSort sort, bool desc)
        {
            return StorageWorkOrderRepository.SortWorkOrders(CompiledGetByActive(db, true), sort, desc);
        }

        public IQueryable<StorageWorkOrder> GetInactive(StorageSort sort, bool desc)
        {
            return StorageWorkOrderRepository.SortWorkOrders(CompiledGetByActive(db, false), sort, desc);
        }

        public IQueryable<StorageWorkOrder> Search(string search)
        {
            var accountRepo = new AccountRepository();
            var franchiseRepo = new FranchiseRepository();
            var accounts = accountRepo.Search(franchiseRepo.GetStorage().FranchiseID, search, 0, 100).Select(i => i.AccountID);
            return (from storage in db.StorageWorkOrders
                where 
                    (accounts.Contains(storage.AccountID) ||
                     storage.Lookup == search)
                    && !storage.CancellationDate.HasValue
                select storage);
        }

        public IQueryable<StorageWorkOrder> GetForInvoice(DateTime date)
        {
            // we invoice 20 days ahead of the due date.
            date = date.AddDays(StorageInvoiceDaysAhead);
            return (from i in db.StorageWorkOrders
                where i.NextInvoiceDate.HasValue
                      && i.NextInvoiceDate.Value <= date
                      && i.MonthlyPayment > 0
                select i);
        }

        public IQueryable<StorageWorkOrder> GetForAutomaticBilling()
        {
            // we want to attempt to charge all cards that failed to run before this date
            var lastBillingAttempt = DateTime.Now.AddDays(-1);
            return (from i in db.StorageWorkOrders
                where i.IsAutomaticBilling
                      && !i.CancellationDate.HasValue
                      && !i.HasPaymentError
                      && i.CreditCardID.HasValue
                      && (!i.LastAutomaticBillingDate.HasValue || i.LastAutomaticBillingDate.Value < lastBillingAttempt)
                select i);
        }

        private IQueryable<StorageInvoice> GetInvoicesDue(DateTime date)
        {
            return CompiledGetDue(db, date);
        }

        public IQueryable<StorageInvoice> GetForPrint()
        {
            var date = DateTime.Now.AddDays(20);
            var invoices = (from i in this.GetInvoicesDue(date)
                where 
                    !i.IsPrinted
                    && !i.IsCancelled 
                    && !i.IsRemoved
                select i);
            return invoices;
        }

        public IQueryable<StorageWorkOrder> GetForPaperlessInvoice()
        {
            return (from i in db.StorageWorkOrders
                where i.EmailInvoices
                      && !i.CancellationDate.HasValue
                      && i.NextInvoiceDate.HasValue
                select i);
        }
    }
}