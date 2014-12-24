namespace Business
{
    public struct GoogleMapsDistance
    {
        /// <summary>
        /// travel distance (in meters)
        /// </summary>
        public decimal value { get; set; }

        /// <summary>
        /// formatted display of travel distance
        /// </summary>
        public string text { get; set; }
    }
}