using System.Collections.Generic;

namespace Moovers.WebModels.Reports
{
    public struct InventoryItemReportItem
    {
        public string item { get; set; }

        public int Count { get; set; }
    }

    public class InventoryItemReport
    {
        public IEnumerable<InventoryItemReportItem> Items { get; set; }

        public IEnumerable<InventoryItemReportItem> CustomItems { get; set; }

        public InventoryItemReport(IEnumerable<InventoryItemReportItem> items, IEnumerable<InventoryItemReportItem> customs)
        {
            this.Items = items;
            this.CustomItems = customs;
        }
    }
 }