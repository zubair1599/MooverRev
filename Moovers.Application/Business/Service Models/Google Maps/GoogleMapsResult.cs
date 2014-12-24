using System.Collections.Generic;

namespace Business
{
    public struct GoogleMapsResult
    {
        public string status { get; set; }

        public IEnumerable<GoogleMapsRoute> routes { get; set; }
    }
}