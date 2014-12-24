using System.Collections.Generic;
using Business.Enums;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public sealed class VaultListModel : SortableModel<VaultSort>
    {
        public override VaultSort Sort
        {
            get;
            set;
        }

        public override bool Desc
        {
            get;
            set;
        }

        public Business.Utility.PagedResult<StorageVault> Vaults { get; set; }

        public override IEnumerable<KeyValuePair<string, VaultSort>> GetHeaders()
        {
            return new Dictionary<string, VaultSort>() {
                { "Work Order", VaultSort.WorkOrder },
                { "Vault", VaultSort.VaultID },
                { "Zone", VaultSort.Zone },
                { "Row", VaultSort.Row },
                { "Shelf", VaultSort.Shelf }
            };
        }

        public VaultListModel(Business.Utility.PagedResult<StorageVault> vaults, VaultSort sort, bool desc)
        {
            this.Sort = sort;
            this.Desc = desc;
            this.Vaults = vaults;
        }
    }
}
