using System;

namespace WebServiceModels
{
    public class UnloadVerficationRepresentation
    {
        public Guid quote_id { get; set; }
        public Guid stop_id { get; set; }
        public int survey_id { get; set; }
        public decimal gratuity { get; set; }
        public SignatureRepresentation signature { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}