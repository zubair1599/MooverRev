using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class StoragePayment
    {
        public IEnumerable<StorageInvoice_Payment_Rel> GetInvoices()
        {
            return this.StorageInvoice_Payment_Rel.Where(i => !i.StorageInvoice.IsCancelled && !i.StorageInvoice.IsRemoved);
        }
    }
}
