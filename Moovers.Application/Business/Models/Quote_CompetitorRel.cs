using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public partial class Quote_Competitor_Rel
    {
        public object ToJsonObject()
        {
            return new
            {
                Name = this.Competitor.IsOther ? this.Name : this.Competitor.Name,
                ID = this.RelID
            };
        }
    }
}
