using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Models;

namespace WebServiceModels
{
    public class QuoteDirectionRepresentation
    {
        public Guid quote_id { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public DateTime checkin_time { get; set; }
    }
}
