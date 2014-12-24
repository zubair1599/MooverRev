using System.Collections.Generic;
using System.Linq;
using Business.Models;
using Business.Repository.Models;

namespace Moovers.WebModels
{
    public class QuoteInventoryModel : QuoteEdit
    {
        public IEnumerable<Box> GetBoxes()
        {
            var repo = new BoxRepository();
            return repo.GetAll();
        }

        public IQueryable<InventoryItem> GetItems()
        {
            var repo = new InventoryItemRepository();
            return repo.GetUnarchived();
        }

        public IEnumerable<InventoryItem> GetCustomItems()
        {
            var repo = new InventoryItemRepository();
            return repo.GetCustomForQuote(this.Quote.QuoteID);
        }

        public QuoteInventoryModel() { }

        public QuoteInventoryModel(Business.Models.Quote quote) : base(quote, "Inventory") { }
    }
}
