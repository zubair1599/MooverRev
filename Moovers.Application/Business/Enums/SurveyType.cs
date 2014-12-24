using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Business.Enums
{
    public enum SurveyType
    {
        [Description("Loved it!")]
        Lovely,

        [Description("Pretty Good")]
        Good,

        [Description("Not so hot!")]
        Bad
    }
}
