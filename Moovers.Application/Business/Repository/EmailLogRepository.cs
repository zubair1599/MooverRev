using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class EmailLogRepository : RepositoryBase<EmailLog>
    {
        private static readonly Func<MooversCRMEntities, Guid, EmailLog> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, EmailLog>(
            (db, id) => db.EmailLogs.SingleOrDefault(i => i.EmailLogID == id)
            );

        public override EmailLog Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }
    }
}