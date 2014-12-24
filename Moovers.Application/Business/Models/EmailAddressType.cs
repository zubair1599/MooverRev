using System.ComponentModel;

namespace Business.Models
{
    public enum EmailAddressType
    {
        [Description("Primary")]
        Primary,

        [Description("Billing")]
        Secondary
    }
}