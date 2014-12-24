using System.Linq;
using Business.Utility;

namespace Business
{
    public struct LatLng
    {
        public double Latitude;

        public double Longitude;

        public override string ToString()
        {
            return this.Latitude + "," + this.Longitude;
        }

        public decimal GetFlyoverDistance(LatLng end)
        {
            var distance = (decimal)GeoCodeCalc.CalcDistance(this.Latitude, this.Longitude, end.Latitude, end.Longitude);
            return distance;
        }

        /// <summary>
        /// Estimates drive time by approximating flyover miles / 50 mph
        /// </summary>
        /// <param name="end"></param>
        /// <returns></returns>
        public decimal GetFlyoverTime(LatLng end)
        {
            var distance = this.GetFlyoverDistance(end);
            return distance / 50m;
        }

        /// <summary>
        /// Travel Distance in Miles - uses a service to calculate actual drive miles.
        /// </summary>
        public decimal GetDistance(LatLng end)
        {
            var maps = DistanceCalculator.GetMapsResult(this, end);
            var distance = maps.routes.First().legs.First().distance.value;
            return General.MetersToMiles(distance);
        }

        /// <summary>
        /// Travel Time -- uses a service to calculate actual estimated drive time.
        /// </summary>
        public decimal GetTime(LatLng end)
        {
            var maps = DistanceCalculator.GetMapsResult(this, end);
            var seconds = maps.routes.First().legs.First().duration.value;
            return Date.SecondsToMinutes(seconds);
        }
    }
}