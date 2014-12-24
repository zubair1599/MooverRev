using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceModels
{
    public class InventoryVerificationRepresentation
    {
        public Guid quote_id { get; set; }
        public Guid stop_id { get; set; }       
        public string email_receipts { get; set; }
        public SignatureRepresentation signature { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}
