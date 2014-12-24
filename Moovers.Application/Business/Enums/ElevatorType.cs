using System.ComponentModel;

namespace Business.Enums
{
    public enum ElevatorType
    {
        [Description("No Elevator")]
        No_Elevator,

        [Description("Public")]
        Public,

        [Description("Reserved")]
        Reserved,
    }
}