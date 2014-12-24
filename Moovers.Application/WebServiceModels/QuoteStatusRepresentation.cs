using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Enums;

namespace WebServiceModels
{
    public class QuoteStatusRepresentation
    {
        public Guid stop_id { get; set; }
        public DateTime status_datetime { get; set; }
        public int status_type { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}
