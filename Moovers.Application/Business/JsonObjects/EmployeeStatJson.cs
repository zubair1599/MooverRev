using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public class EmployeeStatJson
    {
        public bool IsAverage  { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public IEnumerable<decimal> Rates { get; set; }

        public decimal Average { private get; set; }

        public string GetClass()
        {
            if (this.GetAverage() > 45)
            {
                return "text-green";
            }
            
            return "text-red";
        }

        public decimal GetAverage()
        {
            return this.Average;
        }
    }
}
