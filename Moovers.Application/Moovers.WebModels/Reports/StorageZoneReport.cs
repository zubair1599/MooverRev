using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Moovers.WebModels.Reports
{
    public class StorageZoneReport
    {
        public IEnumerable<StorageZone> Zones { get; set; }

        public IEnumerable<StorageVault> Vaults { get; set; }

        public StorageZoneReport(IEnumerable<StorageZone> zones) : this(zones, Enumerable.Empty<Business.Models.StorageVault>())
        {
        }

        public StorageZoneReport(IEnumerable<StorageZone> zones, IEnumerable<StorageVault> vaults)
        {
            this.Zones = zones;
            this.Vaults = vaults;
        }
    }
}
