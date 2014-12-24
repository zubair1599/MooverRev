using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class StorageInvoiceRepository : RepositoryBase<StorageInvoice>
    {
        private static Func<MooversCRMEntities, Guid, StorageInvoice> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, StorageInvoice>(
            (db, id) => db.StorageInvoices.SingleOrDefault(i => i.InvoiceID == id)
            );

        private static Func<MooversCRMEntities, string, StorageInvoice> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, StorageInvoice>(
            (db, id) => db.StorageInvoices.SingleOrDefault(i => i.Lookup == id)
            );

        public override StorageInvoice Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public StorageInvoice Get(string id)
        {
            return CompiledGetByLookup(db, id);
        }
    }
}