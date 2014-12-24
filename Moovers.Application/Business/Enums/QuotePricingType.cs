using System.ComponentModel;

namespace Business.Enums
{
    public enum QuotePricingType
    {
        Binding = 0,

        Hourly = 1,

        [Description("Non Binding")]
        NonBinding = 2
    }
}
