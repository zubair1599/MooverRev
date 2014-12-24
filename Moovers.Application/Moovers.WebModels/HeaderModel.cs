using System;
using System.Linq;
using Business.Repository;
using Business.Repository.Models;

namespace Moovers.WebModels
{
    public class HeaderModel
    {
        private Guid WebQuoteUserID { get; set; }

        public string ParentMenu { get; set; }

        public Business.Models.Franchise Franchise { get; set; }

        public int GetLeadCount()
        {
            // corporate user
            if (this.Franchise == null)
            {
                var repo = new LeadRepository();
                var quoteRepo = new QuoteRepository();
                var leadCount = repo.GetUnreadCount();
                var webCount = quoteRepo.GetOpenForUser(this.WebQuoteUserID).Count();
                return leadCount + webCount;
            }
            else
            {
                var repo = new LeadRepository();
                var quoteRepo = new QuoteRepository();
                var leadCount = repo.GetUnreadCount(this.Franchise.FranchiseID);
                var webCount = quoteRepo.GetOpenForUser(this.Franchise.FranchiseID, this.WebQuoteUserID).Count();
                return leadCount + webCount;
            }
        }

        public int GetUnpostedCount()
        {
            if (this.Franchise == null)
            {
                return 0;
            }

            var repo = new PostingRepository();
            return repo.GetUnpostedCount(this.Franchise.FranchiseID);
        }

        public int GetStorageAlerts()
        {
            if (this.Franchise == null || this.Franchise.HasStorage)
            {
                var repo = new StorageWorkOrderRepository();
                return repo.GetForPrint().Select(i => i.StorageWorkOrder).Distinct().Count();
            }

            return 0;
        }

        public HeaderModel(Business.Models.Franchise franchise, string parentMenu, Guid webQuoteUserID)
        {
            this.ParentMenu = parentMenu;
            this.WebQuoteUserID = webQuoteUserID;
            this.Franchise = franchise;
        }

        public HeaderModel()
        {
            
        }
    }
}