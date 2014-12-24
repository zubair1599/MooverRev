using System.ComponentModel;

namespace Business.Enums
{
    public enum CaseSort
    {
        Status,
        CaseID,
        Shipper,
        Coverage,
        Updated,
        Priority,
        Created,

        [Description("Days Open")]
        DaysOpen,


    }
    public enum CaseStatus
    {
        Pending = 1,
        Inprocess = 2,
        Closed = 3,
    }

    public enum Converage
    {
        Minimum = 1,
        Maximum = 2,
    }
}
