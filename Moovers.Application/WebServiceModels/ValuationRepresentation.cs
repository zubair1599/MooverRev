using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceModels
{
    public class ValuationRepresentation
    {
        public string name { get; set; }
        public Guid valuation_type_id { get; set; }
        public decimal total_weight { get; set; }
        public decimal value { get; set; }
        public decimal? max_coverage { get; set; }
    }
}
