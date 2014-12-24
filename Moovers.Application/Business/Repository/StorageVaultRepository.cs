using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Utility;
using Business.ViewModels;

namespace Business.Repository.Models
{
    public class StorageVaultRepository : RepositoryBase<StorageVault>
    {
        private static readonly Func<MooversCRMEntities, Guid, StorageVault> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, StorageVault>(
            (db, id) => db.StorageVaults.SingleOrDefault(i => i.StorageVaultID == id)
            );

        private static readonly Func<MooversCRMEntities, string, StorageVault> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, StorageVault>(
            (db, id) => db.StorageVaults.SingleOrDefault(i => i.Lookup == id)
            );

        private static readonly Func<MooversCRMEntities, IQueryable<StorageVault>> CompiledGetAll = CompiledQuery.Compile<MooversCRMEntities, IQueryable<StorageVault>>(
            (db) => (from v in db.StorageVaults
                where !v.IsRemoved
                select v)
            );

        public IQueryable<StorageVault> GetAll()
        {
            return CompiledGetAll(db);
        }

        public PagedResult<StorageVault> GetAll(VaultSort sort, bool desc, int page, int pageSize)
        {
            var vaults = (from v in db.StorageVaults
                where !v.IsRemoved
                select v);

            if (sort == VaultSort.Row)
            {
                vaults = vaults.OrderWithDirection(i =>
                    i.StorageWorkOrder_Vault_Rel.Any(r => !r.IsRemoved)
                        ? i.StorageWorkOrder_Vault_Rel.FirstOrDefault(r => !r.IsRemoved).Row
                        : String.Empty, desc);
            }

            if (sort == VaultSort.Shelf)
            {
                vaults = vaults.OrderWithDirection(i =>
                    i.StorageWorkOrder_Vault_Rel.Any(r => !r.IsRemoved)
                        ? i.StorageWorkOrder_Vault_Rel.FirstOrDefault(r => !r.IsRemoved).Shelf
                        : String.Empty, desc);
            }

            if (sort == VaultSort.VaultID)
            {
                vaults = vaults.OrderWithPadding(i => i.Lookup, 10, desc);
            }

            if (sort == VaultSort.WorkOrder)
            {
                vaults = vaults.OrderWithPadding(i => 
                    i.StorageWorkOrder_Vault_Rel.Any(v => !v.IsRemoved) 
                        ? i.StorageWorkOrder_Vault_Rel.FirstOrDefault(v => !v.IsRemoved).StorageWorkOrder.Lookup 
                        : "0", 10, desc);
            }

            if (sort == VaultSort.Zone)
            {
                vaults = vaults.OrderWithDirection(i =>
                    i.StorageWorkOrder_Vault_Rel.Any(r => !r.IsRemoved)
                        ? i.StorageWorkOrder_Vault_Rel.FirstOrDefault(r => !r.IsRemoved).StorageZone.Name
                        : String.Empty, desc);
            }

            return new PagedResult<StorageVault>(vaults, page, pageSize);
        }

        public IEnumerable<StorageVault> GetInZone(Guid zoneid, string row)
        {
            // do basic filtering to reduce thequery size
            var all = (from i in this.GetAll()
                where i.StorageWorkOrder_Vault_Rel.Any(r => r.ZoneID == zoneid && r.Row == row)
                select i).ToList();

            return (from v in all
                where v.GetWorkOrder() != null && v.StorageZone.ZoneID == zoneid && v.Row == row
                select v);
        }

        public StorageVault Get(string lookup)
        {
            return CompiledGetByLookup(db, lookup);
        }

        public override StorageVault Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }
    }
}