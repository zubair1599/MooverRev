using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Moovers.WebModels.Reports
{
    public class StorageWarehouseModel
    {
        public IEnumerable<StorageZone> Zones { get; set; }

        public IEnumerable<StorageVault> Vaults { get; set; }

        public IEnumerable<StorageWorkOrder_InventoryItem_Rel> Overstuffed { get; set; }

        public int GetVaultsIn(Guid zoneid, string row)
        {
            return this.Vaults.Count(i => !i.IsRemoved && i.Row == row && i.StorageZone != null && i.StorageZone.ZoneID == zoneid);
        }

        public int GetOSIn(Guid zoneid, string row)
        {
            return this.Overstuffed.Count(i => !i.IsRemoved && i.OverstuffRow == row && i.OverstuffZoneID == zoneid);
        }

        public StorageWarehouseModel(IEnumerable<StorageZone> zones, IEnumerable<StorageVault> vaults, IEnumerable<StorageWorkOrder_InventoryItem_Rel> overstuffed)
        {
            this.Zones = zones;
            this.Vaults = vaults;
            this.Overstuffed = overstuffed;
        }
    }
}