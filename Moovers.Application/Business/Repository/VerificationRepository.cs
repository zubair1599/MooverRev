using System;
using System.Data.Objects;
using System.Linq;
using Business.Interfaces;
using Business.Models;

namespace Business.Repository
{
    public class VerificationRepository : RepositoryBase<AccountSignature>, IVerificationRepository
    {
        private static readonly Func<MooversCRMEntities, Guid, AccountSignature> CompiledGetByID = CompiledQuery
            .Compile<MooversCRMEntities, Guid, AccountSignature>(
                (db, id) => db.AccountSignatures.Include("InventoryVerification")
                    .Include("Account")
                    .Include("QuoteCustomerSignOff")
                    .Include("UnloadVerification").SingleOrDefault(i => i.SignatureID == id)
            );

        public override AccountSignature Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public bool UpdateQuoteCustomerSignature(QuoteCustomerSignOff customerSignOff)
        {
            if (customerSignOff == null) return false;

            AccountSignature signature = customerSignOff.AccountSignature;
            Quote quote = new QuoteRepository().Get(customerSignOff.QuoteID);

            if (customerSignOff.AccountSignature != null)
            {
                customerSignOff.AccountSignature.AccountID = quote.AccountID;
                customerSignOff.AccountSignature.UpdatedOn = DateTime.Now;
            }
            Add(customerSignOff.AccountSignature);

            Save();
            return true;
        }

        public bool UpdateInventoryVerificationSignature(InventoryVerification inventoryVerification)
        {
            if (inventoryVerification == null) return false;

            AccountSignature signature = inventoryVerification.AccountSignature;
            Quote quote = new QuoteRepository().Get(inventoryVerification.QuoteID);

            if (inventoryVerification.AccountSignature != null)
            {
                inventoryVerification.AccountSignature.AccountID = quote.AccountID;
                inventoryVerification.AccountSignature.UpdatedOn = DateTime.Now;
            }
            Add(inventoryVerification.AccountSignature);

            Save();
            return true;
        }

        public bool UpdateUnloadVerificationSignature(UnloadVerification unloadVerification)
        {
            try
            {
                if (unloadVerification == null) return false;

                AccountSignature signature = unloadVerification.AccountSignature;
                Quote quote = new QuoteRepository().Get(unloadVerification.QuoteID);

                if (unloadVerification.AccountSignature != null)
                {
                    unloadVerification.AccountSignature.AccountID = quote.AccountID;
                    unloadVerification.AccountSignature.UpdatedOn = DateTime.Now;
                }
                Add(unloadVerification.AccountSignature);

                Save();
                return true;
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        public bool UpdateQuoteStatus(QuoteStatu status)
        {
            try
            {
                db.AddToQuoteStatus(status);
                this.Save();
                return true;
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}