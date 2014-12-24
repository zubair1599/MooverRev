using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Enums;
using Business.Models;

namespace WebServiceModels
{
    public class SignatureRepresentation
    {
        public Guid signature_id { get; set; }      
        public string signature { get; set; }
        public DateTime signature_time { get; set; }
        public int signature_type { get; set; }
        public SignatureRepresentation() { }

        public SignatureRepresentation(AccountSignature sign)
        {
            this.signature_id = sign.SignatureID;
            
            this.signature = sign.Signature;
            this.signature_time = sign.SignatureTime;
            this.signature_type = sign.Type;
        }
    }
}
