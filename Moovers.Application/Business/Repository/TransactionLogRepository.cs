using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class TransactionLogRepository : RepositoryBase<TransactionLog>
    {
        private Func<MooversCRMEntities, Guid, TransactionLog> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, TransactionLog>(
            (db, id) => db.TransactionLogs.SingleOrDefault(i => i.TransactionLogID == id)
            );

        public override TransactionLog Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }
    }
}