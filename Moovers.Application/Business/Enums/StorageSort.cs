using System.ComponentModel;

namespace Business.Enums
{
    public enum StorageSort
    {
        [Description("Work Order")]
        Lookup,

        Account,
        Vaults,
        Oversize,
        Balance,

        [Description("Overdue")]
        OverdueBalance,

        [Description("Monthly Payment")]
        MonthlyPayment,

        [Description("Paid Through")]
        NextPayment,

        [Description("Last Payment")]
        LastPayment,

        [Description("DaysOverdue")]
        DaysOverdue,

        [Description("Auto Bill")]
        AutoBill
    }
}