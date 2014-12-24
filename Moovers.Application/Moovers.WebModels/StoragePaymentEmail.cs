namespace Moovers.WebModels
{
    public class StoragePaymentEmail
    {
        public Business.Models.StorageWorkOrder WorkOrder { get; set; }

        public Business.Models.Payment Payment { get; set; }

        public Business.Models.StorageInvoice Invoice { get; set; }
    }
}
