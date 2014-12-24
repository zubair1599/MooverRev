using System.ComponentModel;

namespace Moovers.WebModels
{
    public enum LocationSort
    {
        [Description("Location Name")]
        LocationName,
        [Description("Location Type")]
        LocationType,
        [Description("Store ID")]
        StoreId
    }
}