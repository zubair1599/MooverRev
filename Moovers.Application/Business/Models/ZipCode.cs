using System.Collections.Generic;
using System.Text;
using Business.Utility;

namespace Business.Models
{
    public partial class ZipCode
    {
        public LatLng GetLatLng()
        {
            return new LatLng() {
                Latitude = (double)this.Latitude,
                Longitude = (double)this.Longitude
            };
        }
    }
}
