using System.Text;

namespace Business.Models
{
    public enum AlgorithmTypes
    {
        ItemLoadTime = 0,
        ItemLoadUnloadTime = 1,
        StopTime = 2,
        ItemPrice = 3,
        StopPrice = 4,
        DrivePrice = 5,
        QuotePrice = 6
    }

    public partial class PricingAlgorithm
    {
        public static readonly int LongDistanceMoveMiles = int.Parse(System.Configuration.ConfigurationManager.AppSettings["LongDistanceMileage"]);
    }
}