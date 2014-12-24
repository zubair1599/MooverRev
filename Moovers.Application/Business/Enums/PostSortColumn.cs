using System.ComponentModel;

namespace Business.Enums
{
    public enum PostSortColumn
    {
        [Description("Service Date")]
        ServiceDate,

        [Description("Quote Num")]
        QuoteID,

        [Description("Posting Date")]
        PostingDate,

        [Description("Shipper")]
        AccountName,

        ////[Description("Billing Account")]
        ////BillingAccount,

        [Description("Price")]
        Price,

        [Description("Employees")]
        Employees,

        [Description("Vehicles")]
        Vehicles,

        [Description("Balance")]
        Balance,

        [Description("")]
        LostDebt,

        [Description("")]
        Print
    }
}