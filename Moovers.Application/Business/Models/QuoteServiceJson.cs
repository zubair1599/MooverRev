using System;

namespace Business.Models
{
    public struct QuoteServiceJson
    {
        public string ServiceID { get; set; }

        public Guid QuoteID { get; set; }

        public string Description { get; set; }

        public int Type { get; set; }

        public decimal Price { get; set; }
    }
}