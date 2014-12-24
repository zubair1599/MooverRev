using System;
using System.Linq.Expressions;
using System.Text;

namespace Business.Models
{
    /// <summary>
    /// Each day, Crews = # of trucks can be booked
    /// This class allows overriding the # of crews for any given day.
    /// </summary>
    public partial class CustomCrewCount
    {
        public DateTime ToDate()
        {
            return new DateTime(this.Year, this.Month, this.Day);
        }
    }
}
