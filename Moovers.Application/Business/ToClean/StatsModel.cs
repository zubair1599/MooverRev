using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.ViewModels
{
    public class StatsModel
    {
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public IEnumerable<JsonObjects.EmployeeStatJson> Stats { get; set; }
    }
}
