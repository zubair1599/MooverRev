namespace Business
{
    /// <summary>
    /// C# structures for JSON coming from SmartyStreets
    /// https://github.com/smartystreets/LiveAddressSamples
    /// </summary>
    public class CandidateAddress
    {
        public int input_index { get; set; }

        public int candidate_index { get; set; }

        public string delivery_line_1 { get; set; }

        public string last_line { get; set; }

        public string delivery_point_barcode { get; set; }

        public Components components { get; set; }

        public Metadata metadata { get; set; }

        public Analysis analysis { get; set; }
    }
}