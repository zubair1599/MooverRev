using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class SmartyStreetsCacheRepository : RepositoryBase<SmartyStreetsCache>
    {
        private static readonly Func<MooversCRMEntities, Guid, SmartyStreetsCache> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, SmartyStreetsCache>(
            (db, id) => db.SmartyStreetsCaches.SingleOrDefault(i => i.CacheID == id)
            );

        private static readonly Func<MooversCRMEntities, Address, SmartyStreetsCache> CompiledGetByAddress = CompiledQuery.Compile<MooversCRMEntities, Address, SmartyStreetsCache>(
            (db, address) => (db.SmartyStreetsCaches.Where(c => c.Street.Equals((address.Street1 + " " + (address.Street2 ?? String.Empty)).Trim(), StringComparison.OrdinalIgnoreCase)
                                                                && c.City.Equals(address.City, StringComparison.OrdinalIgnoreCase)
                                                                && c.State.Equals(address.State, StringComparison.OrdinalIgnoreCase)
                                                                && c.ZipCode.Equals(address.Zip, StringComparison.OrdinalIgnoreCase))).FirstOrDefault()
            );

        public override SmartyStreetsCache Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public SmartyStreetsCache GetFromAddress(Address address)
        {
            return CompiledGetByAddress(db, address);
        }
    }
}