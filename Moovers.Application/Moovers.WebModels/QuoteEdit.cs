using System;
using System.Collections.Generic;
using Business.Models;
using Business.Repository.Models;

namespace Moovers.WebModels
{
    public class QuoteEdit
    {
        public Business.Models.Quote Quote { get; set; }

        public IEnumerable<aspnet_User> Users { get; set; }

        public IEnumerable<Franchise> Franchises { get; set; }

        public IEnumerable<Competitor> Competitors { get; set; }

        public string AccountJson
        {
            get
            {
                return Business.Utility.LocalExtensions.SerializeToJson(this.Quote.Account.ToMiniJsonObject());
            }
        }

        public string Tab { get; set; }

        private string[] PageOrder = { "Stops", "Inventory", "Pricing", "Schedule", "Overview" };

        public IEnumerable<EmailTemplate> GetEmailTemplates()
        {
            return new EmailTemplateRepository().GetAll();
        }

        public string GetBackAction()
        {
            var idx = Array.IndexOf(PageOrder, this.Tab);
            if (idx == 0)
            {
                return null;
            }

            return PageOrder[idx - 1];
        }

        public string GetForwardAction()
        {
            var idx = Array.IndexOf(PageOrder, this.Tab);
            if (idx == PageOrder.Length - 1)
            {
                return null;
            }

            return PageOrder[idx + 1];
        }

        public QuoteEdit()
        {
            var repo = new aspnet_UserRepository();
            var users = repo.GetAll();

            var franchiseRepo = new FranchiseRepository();
            this.Users = users;
            this.Quote = new Business.Models.Quote();
            this.Franchises = franchiseRepo.GetAll();
        }

        public QuoteEdit(Business.Models.Quote quote, string tab)
            : this()
        {
            var competitorRepo = new CompetitorRepository();
            this.Competitors = competitorRepo.GetAll(quote.FranchiseID);
            this.Quote = quote;
            this.Tab = tab;
        }
    }
}
