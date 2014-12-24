using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class BoxRepository : RepositoryBase<Box>
    {
        public override Box Get(Guid id)
        {
            return db.Boxes.SingleOrDefault(i => i.BoxTypeID == id);
        }

        public IEnumerable<Box> GetAll()
        {
            return db.Boxes.OrderBy(i => i.Name);
        }
    }
}