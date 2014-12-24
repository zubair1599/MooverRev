using System.ComponentModel;

namespace Business.Enums
{
    public enum CrewStatus
    {
        [Description("-- Status --")]
        Default = 1,

        [Description("Out of Town")]
        OutOfTown,

        [Description("Clean Up")]
        CleanUp,

        Van,
        Pickup,
        Delivery,

        Unavailable
    }
}