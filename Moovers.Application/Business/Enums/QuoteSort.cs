using System.ComponentModel;

namespace Business.Enums
{
    public enum QuoteSort
    {
        [Description("Num")]
        QuoteID,

        [Description("Customer")]
        Account,

        [Description("Move Date")]
        Date,

        [Description("Countdown")]
        DaysToMove,

        [Description("Price")]
        Price,

        [Description("Type")]
        Type,

        [Description("Owner")]
        SalesID,

        [Description("Last Modified")]
        LastModified,

        [Description("Survey")]
        VisualSurvey,

        [Description("Status")]
        Status
    }
}