using System;

namespace Business.Utility
{
    public static class GeoCodeCalc
    {
        /// <summary>
        /// Earth Radius (miles)
        /// </summary>
        private const double EarthRadius = 3956.0;

        /// <summary>
        /// Degrees to radians
        /// </summary>
        private static double ToRadian(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        private static double DiffRadian(double val1, double val2)
        {
            return ToRadian(val2) - ToRadian(val1);
        }

        public static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
        {
            // http://www.movable-type.co.uk/scripts/latlong.html
            // φ = latitude, λ = longitude, R = earth’s radius, d = distance
            // a = sin²(Δφ/2) + cos(φ1).cos(φ2).sin²(Δλ/2)
            // c = 2.atan2(√a, √(1−a))
            // d = R.c

            // sin²(Δφ/2)
            var lat = Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0);

            // sin²(Δλ/2)
            var lng = Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0);

            // a = sin²(Δφ/2) + cos(φ1).cos(φ2).sin²(Δλ/2)
            var a = lat + (Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * lng);

            // c = 2.atan2(√a, √(1−a)
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            // d = R.c
            return EarthRadius * c;
        }
    }
}