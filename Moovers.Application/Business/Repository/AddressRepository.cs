using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class AddressRepository : RepositoryBase<Address>
    {
        public override Address Get(Guid id)
        {
            return db.Addresses.SingleOrDefault(a => a.AddressID == id);
        }
    }
}