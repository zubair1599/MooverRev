using System;
using System.Collections.Generic;

namespace Moovers.WebModels
{
    public class FullScheduleModel
    {
        public IEnumerable<DateTime> CustomCrews { get; set; }

        public FullScheduleModel(IEnumerable<DateTime> customCrews)
        {
            this.CustomCrews = customCrews;
        }
    }
}
