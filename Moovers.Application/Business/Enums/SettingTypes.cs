using System.ComponentModel;

namespace Business.Enums
{
    public enum SettingTypes
    {
        [Description("Max-Truck-Schedule")]
        MaxTrucks,

        [Description("Hourly-Person-Destination-Multiplier")]
        HourlyManDestination,
        
        [Description("Hourly-Person-Multiplier")]
        HourlyMan,

        [Description("Hourly-Truck-Destination-Multiplier")]
        HourlyTruckDestination,

        [Description("Hourly-Truck-Multiplier")]
        HourlyTruck,

        [Description("Overnight-Storage")]
        OvernightStorage
    }
}