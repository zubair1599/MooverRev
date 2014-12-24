using System.ComponentModel;

namespace Business.FirstData
{
    public enum FirstDataTransactionTypes
    {
        [Description("00")]
        Purchase,

        [Description("01")]
        PreAuthorization,

        [Description("02")]
        PreAuthorizationCompletion,

        [Description("03")]
        ForcedPost,

        [Description("04")]
        Refund,

        [Description("05")]
        PreAuthorizationOnly,

        [Description("13")]
        Void,

        [Description("32")]
        TaggedPreAuthorizationCompletion,

        [Description("33")]
        TaggedVoid,

        [Description("34")]
        TaggedRefund
    }
}