using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class StorageStatementRepository : RepositoryBase<StorageStatement>
    {
        public override StorageStatement Get(Guid id)
        {
            return db.StorageStatements.SingleOrDefault(i => i.StatementID == id);
        }
    }
}