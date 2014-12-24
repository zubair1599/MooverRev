using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Business.Enums
{
    public enum WorkingType
    {
        [Description("As Needed")]
        Needed =0,

        [Description("Regular")]
        Regular =1
    }

    public enum ClassType {

        [Description("Full Time")]
        FullTime =0,

        [Description("Part Time")]
        PartTime=1

    }
}
