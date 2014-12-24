using System.ComponentModel;

namespace Business.Enums
{
    public enum QuoteStatus
    {
        Open = 1, // staff member came in and quoted a customer
        Scheduled = 2, // put on the schedule 
        Lost = 3, // a different company used, decided to move themselves, etc.
        Cancelled = 4, // scheduled then cancelled
        Deferred = 5, // customer is moving at a different date

        [Description("Won")]
        Completed = 6, // finished move, customer paid

        Duplicate
    }
}