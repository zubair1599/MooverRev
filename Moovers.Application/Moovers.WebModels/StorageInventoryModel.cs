using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class StorageInventoryModel
    {
        public Business.Models.StorageWorkOrder WorkOrder { get; set; }

        public IEnumerable<InventoryItem> Items { get; set; }

        public IEnumerable<StorageZone> Zones { get; set; }

        public StorageInventoryModel(Business.Models.StorageWorkOrder workOrder, IEnumerable<InventoryItem> items, IEnumerable<StorageZone> zones)
        {
            this.WorkOrder = workOrder;
            this.Items = items;
            this.Zones = zones;
        }
    }
}
