using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Repository.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public sealed class StorageList : SortableModel<StorageSort>
    {
        public PagedResult<StorageWorkOrder> WorkOrders { get; set; }

        public override StorageSort Sort { get; set; }

        public override bool Desc { get; set; }

        public override IEnumerable<KeyValuePair<string, StorageSort>> GetHeaders()
        {
            return new Dictionary<string, StorageSort>()
            {
                {StorageSort.Lookup.GetDescription(), StorageSort.Lookup},
                {StorageSort.Account.GetDescription(), StorageSort.Account},
                {StorageSort.Vaults.GetDescription(), StorageSort.Vaults},
                {StorageSort.Oversize.GetDescription(), StorageSort.Oversize},
                {StorageSort.Balance.GetDescription(), StorageSort.Balance},
                {StorageSort.OverdueBalance.GetDescription(), StorageSort.OverdueBalance},
                {StorageSort.MonthlyPayment.GetDescription(), StorageSort.MonthlyPayment},
                {StorageSort.LastPayment.GetDescription(), StorageSort.LastPayment},
                {StorageSort.NextPayment.GetDescription(), StorageSort.NextPayment},
                {StorageSort.DaysOverdue.GetDescription(), StorageSort.DaysOverdue},
                {StorageSort.AutoBill.GetDescription(), StorageSort.AutoBill}
            };
        }

        public StorageList(PagedResult<StorageWorkOrder> workOrders, StorageSort sort, bool desc)
        {
            this.WorkOrders = workOrders;
            this.Sort = sort;
            this.Desc = desc;
        }

        public int GetStorageAlerts()
        {
            var repo = new StorageWorkOrderRepository();
            return repo.GetForPrint().Select(i => i.StorageWorkOrder).Distinct().Count();
        }
    }
}

