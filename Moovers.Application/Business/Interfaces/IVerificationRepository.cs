using System;
using Business.Models;

namespace Business.Interfaces
{
    public interface IVerificationRepository
    {
        AccountSignature Get(Guid id);
        bool UpdateQuoteCustomerSignature(QuoteCustomerSignOff customerSignOff);
        bool UpdateInventoryVerificationSignature(InventoryVerification inventoryVerification);
        bool UpdateUnloadVerificationSignature(UnloadVerification unloadVerificationVerification);
        bool UpdateQuoteStatus(QuoteStatu status);
    }
}