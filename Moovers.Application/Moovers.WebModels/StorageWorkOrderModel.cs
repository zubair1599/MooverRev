using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class StorageWorkOrderModel
    {
        public Business.Models.StorageWorkOrder WorkOrder { get; set; }

        public IEnumerable<StorageVault> StorageVaults { get; set; }

        public StorageWorkOrderModel(Business.Models.StorageWorkOrder workOrder, IEnumerable<StorageVault> vaults)
        {
            this.WorkOrder = workOrder;
            this.StorageVaults = vaults;
        }
    }
}
