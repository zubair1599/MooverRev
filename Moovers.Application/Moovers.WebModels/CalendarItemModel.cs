namespace Moovers.WebModels
{
    public class CalendarItemModel
    {
        public string title { get; set; }

        public double start { get; set; }

        public double end { get; set; }

        public decimal cost { get; set; }

        public bool allDay { get; set; }
    }
}
