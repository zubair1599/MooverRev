using System.ComponentModel;

namespace Business.FirstData
{
    public enum FirstDataCardTypes
    {
        [Description("Visa")]
        Visa,

        [Description("Mastercard")]
        Mastercard,

        [Description("American Express")]
        Amex,

        [Description("Discover")]
        Discover,

        [Description("Diners Club")]
        DinersClub,

        [Description("JCB")]
        JCB
    }
}