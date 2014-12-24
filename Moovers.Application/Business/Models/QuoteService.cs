using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public partial class QuoteService
    {
        public QuoteServiceJson ToJsonObject()
        {
            return new QuoteServiceJson() {
                Description = this.Description,
                QuoteID = this.QuoteID,
                ServiceID = this.ServiceID.ToString(),
                Type = this.Type,
                Price = this.Price
            };
        }
    }
}
