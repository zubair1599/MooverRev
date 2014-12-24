using System.Collections.Generic;
using System.Text;
using System.Web;
using Business.Enums;
using Business.ToClean.QuoteHelpers;

namespace Business.Models
{
    public partial class Claim
    {
        public ClaimType ClaimTypeDisplayName
        {
            get
            {
                return (ClaimType)this.ClaimType;
            }

        }

    }
}
