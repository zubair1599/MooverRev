namespace Business
{
    public struct GoogleMapsStep
    {
        public GoogleMapsDistance distance { get; set; }

        public GoogleMapsDuration duration { get; set; }

        public GoogleMapsLocation end_location { get; set; }

        public GoogleMapsLocation start_location { get; set; }

        public string html_instructions { get; set; }

        // ReSharper disable once UnusedMember.Local
        string travel_mode { get; set; }
    }
}