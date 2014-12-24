using System.ComponentModel;

namespace Moovers.WebModels
{
    public enum EmployeeSort
    {
        Status,
        Number,
        Position,
        Name,
        [Description("Job Title")]
        JobTitle,
        Type,
        Class,
        [Description("YTD Hours")]
        YTD,
        [Description("Last Worked")]
        LastWorked,
        Store,
        IsDriver,
        Wage,

        [Description("Primary Phone")]
        PrimaryPhone,

        [Description("Secondary Phone")]
        SecondaryPhone,

        [Description("Employee ID")]
        EmployeeID
    }
}