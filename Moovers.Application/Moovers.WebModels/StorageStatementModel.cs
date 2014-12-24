using System;
using System.Collections.Generic;
using Business.Models;

namespace Moovers.WebModels
{
    public class StorageStatementModel
    {
        public Business.Models.Franchise Franchise { get; set; }

        public IEnumerable<StorageWorkOrder> StorageWorkOrders { get; set; }

        public DateTime Date { get; set; }
    }
}
