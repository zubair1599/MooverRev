using System.Text;

namespace Business.Models
{
    public partial class Vehicle
    {
        public object ToJsonObject()
        {
            return new
            {
                this.VehicleID,
                this.Lookup,
                this.Name
            };
        }
    }
}
