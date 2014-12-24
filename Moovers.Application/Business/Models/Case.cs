using System.Collections.Generic;
using System.Text;
using Business.Enums;
using Business.Repository.Models;

namespace Business.Models
{
    public partial class Case
    {
        public CaseStatus CaseStatus
        {
            get
            {
                return (CaseStatus) this.Status;
            }
        }

        public Converage converage
        {
            get
            {
                return (Converage)this._Coverage;
            }

            set
            {
                this.converage = value;
            }
        }

        public string ShipperName()
        {
            var repo = new CaseRepository();
            return repo.GetShipperName(this.QuoteID);
        }
    }
}
