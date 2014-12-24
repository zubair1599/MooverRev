using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Interfaces
{
    public interface IPosting
    {
        Guid PostingID { get; }

        bool IsComplete { get; }

        Models.Schedule Schedule { get; }

        DateTime? DateCompleted { get; }

        IEnumerable<Models.Employee> GetEmployees();

        IEnumerable<Models.Vehicle> GetVehicles();

        Models.Quote Quote { get; }

        string Lookup { get; }
    }
}
