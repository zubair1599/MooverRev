using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Business.Models;
using Business.Repository;
using Business.Utility;

namespace Moovers.WebModels
{
    public class FieldStatusViewModel
    {
        public List<QuoteStatu> StatusesList { get; set; }
        public List<InventoryVerification> InventoryVerifications { get; set; }
        public List<UnloadVerification> UnloadVerifications { get; set; }
        public List<QuoteCustomerSignOff> SignOffs { get; set; }
        public Quote Quote { get; set; }

        
        public FieldStatusViewModel(string quoteId)
        {
            if(this.StatusesList == null) this.StatusesList = new List<QuoteStatu>();
            var repo = new QuoteRepository();
            this.Quote = repo.Get(quoteId);
            this.Quote.Stops.ToList().ForEach(s => this.StatusesList.AddRange(s.QuoteStatus));
            this.InventoryVerifications = this.Quote.InventoryVerifications.ToList();
            this.UnloadVerifications = this.Quote.UnloadVerifications.ToList();
            this.SignOffs = this.Quote.QuoteCustomerSignOffs.ToList();           
        }
    }
}
