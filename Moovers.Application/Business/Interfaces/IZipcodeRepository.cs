using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Interfaces
{
    public interface IZipcodeRepository : IRepository<Models.ZipCode>
    {
        Models.ZipCode GetByFirst3(string zip);

        Models.ZipCode Get(string zip);
    }
}
