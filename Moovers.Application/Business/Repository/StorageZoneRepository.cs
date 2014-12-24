using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class StorageZoneRepository : RepositoryBase<StorageZone>
    {
        private static readonly Func<MooversCRMEntities, Guid, StorageZone> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, StorageZone>(
            (db, id) => db.StorageZones.SingleOrDefault(i => i.ZoneID == id)
            );

        private static readonly Func<MooversCRMEntities, IEnumerable<StorageZone>> CompiledGetByAll = CompiledQuery.Compile<MooversCRMEntities, IEnumerable<StorageZone>>(
            (db) => db.StorageZones.OrderBy(i => i.Name)
            );

        public override StorageZone Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public IEnumerable<StorageZone> GetAll()
        {
            return CompiledGetByAll(db);
        }
    }
}