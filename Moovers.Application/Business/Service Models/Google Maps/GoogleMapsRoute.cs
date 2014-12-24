using System.Collections.Generic;

namespace Business
{
    public struct GoogleMapsRoute
    {
        public dynamic bounds { get; set; }

        ////public string copyrights { get; set; }

        ////public dynamic overview_polyline { get; set; }

        public string summary { get; set; }

        public IEnumerable<dynamic> warnings { get; set; }

        public IEnumerable<dynamic> waypoint_order { get; set; }

        public IEnumerable<GoogleMapsLeg> legs { get; set; }
    }
}