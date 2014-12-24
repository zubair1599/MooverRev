using System;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class StorageVault
    {
        private StorageWorkOrder_Vault_Rel GetRel()
        {
            return this.StorageWorkOrder_Vault_Rel.FirstOrDefault(i => !i.IsRemoved && !i.StorageWorkOrder.CancellationDate.HasValue);
        }

        public string Row
        {
            get
            {
                if (this.GetRel() != null)
                {
                    return this.GetRel().Row;
                }

                return String.Empty;
            }
        }

        public string Shelf
        {
            get
            {
                if (this.GetRel() != null)
                {
                    return this.GetRel().Shelf;
                }

                return String.Empty;
            }
        }

        public StorageZone StorageZone
        {
            get
            {
                if (this.GetRel() != null)
                {
                    return this.GetRel().StorageZone;
                }

                return default(StorageZone);
            }
        }

        public StorageWorkOrder GetWorkOrder()
        {
            var rel = this.GetRel();
            if (rel != null)
            {
                return rel.StorageWorkOrder;
            }

            return default(StorageWorkOrder);
        }

        public bool IsUsed()
        {
            return this.GetRel() != null;
        }
    }
}
