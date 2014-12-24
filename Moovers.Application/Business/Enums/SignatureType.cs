using System.ComponentModel;

namespace Business.Enums
{
    public enum SignatureType
    {
        [Description("Customer Signoff")] 
        CustomerSignoff,
        InventoryVerifySignoff,
        TermsAndConditions,
        CustomerOutOfHomeAcknowledge,
        UnloadVerifySignoff,
        Payment
    }
}
