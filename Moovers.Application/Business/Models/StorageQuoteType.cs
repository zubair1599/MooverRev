using System.ComponentModel;

namespace Business.Models
{
    public enum StorageQuoteType
    {
        [Description("Move In")]
        MoveIn = 0,

        [Description("Move Out")]
        MoveOut = 1,

        [Description("Partial Move Out")]
        PartialMoveOut = 2,

        [Description("Storage Access")]
        StorageAccess = 3
    }
}