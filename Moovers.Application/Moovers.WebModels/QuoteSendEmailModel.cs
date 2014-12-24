using System.Collections.Generic;
using System.Linq;
using Business.Utility;

namespace Moovers.WebModels
{
    public class QuoteSendEmailModel : QuoteEdit
    {
        public string UserEmail { get; set; }

        public string GetDefaultTo()
        {
            if (this.GetEmailAddresses().Any())
            {
                return this.GetEmailAddresses().First().Value;
            }

            return "";
        }

        public Dictionary<string, string> GetEmailAddresses()
        {
            var account = this.Quote.Account;
            var emails = new [] {
                Business.Models.EmailAddressType.Primary,
                Business.Models.EmailAddressType.Secondary
            };

            var ret = new Dictionary<string, string>();
            foreach (var e in emails)
            {
                var email = account.GetEmail(e);
                if (email != null)
                {
                    ret.Add(e.GetDescription(), email.Email);
                }
            }

            return ret;
        }

        public QuoteSendEmailModel(Business.Models.Quote quote, string userEmail)
            : base(quote, "Overview")
        {
            this.UserEmail = userEmail;
        }
    }
}
