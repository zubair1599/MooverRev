using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Interfaces
{
    public interface IPaymentRepository : IRepository<Business.Models.Payment>
    {
        string GetUniqueTransactionID();
    }
}
