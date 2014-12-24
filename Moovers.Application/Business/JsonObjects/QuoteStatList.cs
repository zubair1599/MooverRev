using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public struct QuoteStatList
    {
        public int OpenCount { get; set; }

        public decimal OpenAmount { get; set; }

        public int UnassignedCount { get; set; }

        public int ExpiringCount { get; set; }

        public int ScheduledCount { get; set; }

        public decimal ScheduledAmount { get; set; }

        public int Lost30DaysCount { get; set; }

        public decimal Lost30DaysAmount { get; set; }

        public int Won30DaysCount { get; set; }

        public decimal Won30DaysAmount { get; set; }

        public int DeferredCount { get; set; }

        public decimal DeferredAmount { get; set; }
    }
}
