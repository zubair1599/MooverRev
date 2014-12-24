using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class FileRepository : RepositoryBase<File>
    {
        public override File Get(Guid id)
        {
            return db.Files.SingleOrDefault(i => i.FileID == id);
        }
    }
}