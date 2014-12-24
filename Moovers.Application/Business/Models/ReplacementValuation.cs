using System;
using System.Text;

namespace Business.Models
{
    public partial class ReplacementValuation
    {
        public string DisplayCost()
        {
            if (this.Cost == 0)
            {
                return "Free";
            }

            return String.Format("{0:C}", this.Cost);
        }
    }
}