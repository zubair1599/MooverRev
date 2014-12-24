using System.ComponentModel;
using Business.ToClean.QuoteHelpers;

namespace Business.Enums
{
    public enum StopAddressType
    {
        [OptGroup("Residential")]
        House,

        [OptGroup("Residential")]
        Apartment,

        [OptGroup("Residential")]
        Condo,

        [OptGroup("Residential")]
        Multiplex,

        [OptGroup("Residential")]
        [Description("Senior Housing")]
        Senior_Housing,

        [OptGroup("Residential")]
        [Description("Mobile Home")]
        Mobile_Home,

        [OptGroup("Commercial")]
        Storage,

        [OptGroup("Commercial")]
        [Description("Moover's Storage")]
        MooversStorage,

        [OptGroup("Commercial")]
        Warehouse,

        [OptGroup("Commercial")]
        Office,

        [OptGroup("Commercial")]
        Retail,

        [OptGroup("Commercial")]
        Industrial,

        [OptGroup("Government")]
        [Description("Military Base")]
        Military_Base
    }
}