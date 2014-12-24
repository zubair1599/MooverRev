using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class StorageWorkOrder_InventoryItem_RelRepository : RepositoryBase<StorageWorkOrder_InventoryItem_Rel>
    {
        public override StorageWorkOrder_InventoryItem_Rel Get(Guid id)
        {
            return db.StorageWorkOrder_InventoryItem_Rel.SingleOrDefault(i => i.RelID == id);
        }

        public IEnumerable<StorageWorkOrder_InventoryItem_Rel> GetOverstuffed()
        {
            return db.StorageWorkOrder_InventoryItem_Rel.Where(i => i.IsOverstuffed && !i.IsRemoved);
        }
    }
}