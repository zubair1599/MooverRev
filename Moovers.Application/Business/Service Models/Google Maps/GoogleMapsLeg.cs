using System.Collections.Generic;

namespace Business
{
    public struct GoogleMapsLeg
    {
        public GoogleMapsDistance distance { get; set; }

        public GoogleMapsDuration duration { get; set; }

        public string end_address { get; set; }

        public GoogleMapsLocation end_location { get; set; }

        public string start_address { get; set; }

        public GoogleMapsLocation start_location { get; set; }

        public IEnumerable<GoogleMapsStep> steps { get; set; }

        public IEnumerable<dynamic> via_waypoint { get; set; }
    }
}