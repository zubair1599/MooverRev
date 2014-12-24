using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Interfaces
{
    public interface IFranchiseRepository : IRepository<Models.Franchise>
    {
        Models.Franchise GetClosestTo(Models.ZipCode zip);

    }
}
