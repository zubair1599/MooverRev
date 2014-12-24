using System.ComponentModel;

namespace Business.Enums
{
    public enum RoomType
    {
        // These are mainly used to populate a dropdown
        Attic,

        Basement,

        Bathroom,

        Bedroom,

        [Description("Dining Room")]
        Dining_Room,

        Garage,

        Kitchen,

        [Description("Living Room")]
        Living_Room,

        [Description("Laundry Room")]
        Laundry_Room,

        Office,

        Outdoor,

        [Description("Other (Please specify)")]
        Other
    }
}