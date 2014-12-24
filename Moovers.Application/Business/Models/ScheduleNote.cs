using System;
using System.Text;

namespace Business.Models
{
    public partial class ScheduleNote
    {
        public DateTime ToDate()
        {
            return new DateTime(this.Year, this.Month, this.Day);
        }
    }
}
