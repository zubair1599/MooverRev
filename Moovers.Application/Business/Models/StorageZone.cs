using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class StorageZone
    {
        public IEnumerable<string> GetRows()
        {
            var vaults = this.StorageWorkOrder_Vault_Rel.Where(i => !i.IsRemoved).ToList();
            var os = this.StorageWorkOrder_InventoryItem_Rel.Where(i => i.IsOverstuffed && !i.IsRemoved).ToList();

            if (vaults.Any())
            {
                return vaults.Select(i => i.Row).Distinct().OrderBy(i => i);
            }

            if (os.Any())
            {
                return os.Where(i => !String.IsNullOrEmpty(i.OverstuffRow)).Select(i => i.OverstuffRow).Distinct();
            }

            return Enumerable.Empty<string>();
        }
    }
}
