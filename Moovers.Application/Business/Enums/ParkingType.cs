using System.ComponentModel;

namespace Business.Enums
{
    public enum ParkingType
    {
        Driveway,

        Street,

        [Description("Loading Dock")]
        Loading_Dock,

        [Description("Parking Lot")]
        Parking_Lot
    }
}