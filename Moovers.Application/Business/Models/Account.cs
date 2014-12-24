using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Dynamic;
using Business.Repository;
using Business.Repository.Models;
using LinqKit;
using System.Text.RegularExpressions;

namespace Business.Models
{
    public partial class Account
    {
        public abstract string DisplayName { get; }

        public abstract string DisplayNameLastFirst { get; }

        public virtual dynamic ToMiniJsonObject()
        {
            var billingAddress = this.GetAddress(AddressType.Billing);
            var mailingAddress = this.GetAddress(AddressType.Mailing);
            var primaryPhone = this.GetPhone(PhoneNumberType.Primary);
            var secondaryPhone = this.GetPhone(PhoneNumberType.Secondary);
            var faxPhone = this.GetPhone(PhoneNumberType.Fax);
            var primaryEmail = this.GetEmail(EmailAddressType.Primary);
            var secondaryEmail = this.GetEmail(EmailAddressType.Secondary);

            dynamic ret = new ExpandoObject();
            ret.AccountID = this.AccountID;
            ret.Lookup = this.Lookup;
            ret.Created = this.Created;
            ret.Type = this.DisplayType();
            ret.BillingAddress = (billingAddress == null) ? null : billingAddress.ToJsonObject();
            ret.MailingAddress = (mailingAddress == null) ? null : mailingAddress.ToJsonObject();
            ret.PrimaryPhone = (primaryPhone == null) ? null : primaryPhone.ToJsonObject();
            ret.SecondaryPhone = (secondaryPhone == null) ? null : secondaryPhone.ToJsonObject();
            ret.FaxPhone = (faxPhone == null) ? null : faxPhone.ToJsonObject();
            ret.PrimaryEmail = (primaryEmail == null) ? null : primaryEmail.ToJsonObject();
            ret.SecondaryEmail = (secondaryEmail == null) ? null : secondaryEmail.ToJsonObject();
            ret.CopyMailing = (billingAddress != null) && billingAddress.SameAs(mailingAddress);
            ret.DisplayName = this.DisplayName;

            return ret;
        }

        public virtual dynamic ToJsonObject()
        {
            dynamic ret = ToMiniJsonObject();
            ret.WorkOrders = this.GetWorkOrders().Select(i => i.ToJsonObject());
            ret.Opportunities = this.GetOpportunities().Select(o => o.ToJsonObject());
            ret.Leads = this.GetLeads().Select(l => l.ToJsonObject());
            ret.Cards = this.GetCreditCards().Select(i => i.ToJsonObject());
            return ret;
        }

        public IEnumerable<Account_CreditCard> GetCreditCards()
        {
            return this.Account_CreditCard.Where(i => !i.IsRemoved);
        }

        /// <summary>
        /// Add a card associated with "Franchise" 
        /// </summary>
        /// <param name="franchiseID">NOTE: Often accounts exist for several franchises, so we have to be able to have credit cards associated with many franchises</param>
        /// <param name="token"></param>
        public Account_CreditCard AddCreditCard(Guid franchiseID, FirstData.TransactionResult token)
        {
            var rel = new Account_CreditCard
            {
                AccountID = this.AccountID,
                FranchiseID = franchiseID,
                CardType = token.CardType
            };
            rel.SetCard(token.TransarmorToken);
            rel.SetExpiration(token.Expiry_Date);
            rel.SetCardHolder(token.CardHoldersName);
            this.Account_CreditCard.Add(rel);
            return rel;
        }

        private Account_CreditCard AddCreditCard(Franchise franchise, string name, string cardnumber, string expirationmonth, string expirationyear, string cvv2, string billingZip)
        {
            cardnumber = cardnumber.Replace("-", String.Empty);

            var payment = new Utility.CreditCardPayment();
            var token = payment.GetCreditCardToken(franchise, name, cardnumber, expirationmonth, expirationyear, cvv2, billingZip, this.Lookup);
            return AddCreditCard(franchise.FranchiseID, token);
        }

        public Account_CreditCard AddCreditCard(Franchise franchise, ViewModels.AddPaymentViewModel paymentModel)
        {
            return this.AddCreditCard(franchise, paymentModel.name, paymentModel.cardnumber, paymentModel.expirationmonth, paymentModel.expirationyear, paymentModel.cvv2, paymentModel.billingZip);
        }

        public void SetAddress(Address address, AddressType type)
        {
            var current = this.GetAddress(type);
            if (current != null)
            {
                address.CopyTo(current);
                return;
            }

            if (address.Created == default(DateTime))
            {
                address.Created = DateTime.Now;
            }

            var rel = new Account_Address_Rel
            {
                Address = address,
                Type = (int)type
            };
            this.Account_Address_Rel.Add(rel);
        }

        public void SetPhoneNumber(PhoneNumber number, PhoneNumberType type)
        {
            number.Number = Regex.Replace(number.Number, @"[^\d]", String.Empty);

            var current = this.GetPhone(type);
            if (current != null)
            {
                current.Number = number.Number;
                current.Extension = number.Extension;
                return;
            }

            if (number.Created == default(DateTime))
            {
                number.Created = DateTime.Now;
            }

            var sort = 0;
            if (this.Account_PhoneNumber_Rel.Any())
            {
                sort = this.Account_PhoneNumber_Rel.Max(p => p.Sort);
            }

            var rel = new Account_PhoneNumber_Rel
            {
                PhoneNumber = number,
                Sort = sort,
                Type = (int)type
            };
            this.Account_PhoneNumber_Rel.Add(rel);
        }

        public void SetEmail(EmailAddress email, EmailAddressType type)
        {
            var current = this.GetEmail(type);
            if (current != null)
            {
                current.Email = email.Email;
                return;
            }

            if (email.Created == default(DateTime))
            {
                email.Created = DateTime.Now;
            }

            var sort = 0;
            if (this.Account_EmailAddress_Rel.Any())
            {
                sort = this.Account_EmailAddress_Rel.Max(e => e.Sort);
            }

            var rel = new Account_EmailAddress_Rel
            {
                EmailAddress = email,
                Sort = sort,
                Type = (int)type
            };
            this.Account_EmailAddress_Rel.Add(rel);
        }

        public string DisplayType()
        {
            if (this is PersonAccount)
            {
                return "Person";
            }

            if (this is BusinessAccount)
            {
                return "Business";
            }

            return "Account";
        }

        private IEnumerable<Quote> GetOpportunities()
        {
            var repo = new QuoteRepository();
            return repo.GetForAccount(this.AccountID).OrderByDescending(o => o.Created);
        }

        private IEnumerable<Lead> GetLeads()
        {
            var repo = new LeadRepository();
            return repo.GetbyAccount(this.AccountID);
        }
        private IEnumerable<StorageWorkOrder> GetWorkOrders()
        {
            var repo = new StorageWorkOrderRepository();
            return repo.GetForAccount(this.AccountID).OrderByDescending(i => i.StartDate);
        }

        public EmailAddress GetEmail(EmailAddressType type)
        {
            var rel = this.Account_EmailAddress_Rel.FirstOrDefault(e => e.Type == (int)type);
            if (rel == null)
            {
                return null;
            }

            return rel.EmailAddress;
        }

        public Address GetAddress(AddressType type)
        {
            var rel = this.Account_Address_Rel.FirstOrDefault(e => e.Type == (int)type);
            if (rel == null)
            {
                return null;
            }

            return rel.Address;
        }

        public PhoneNumber GetPhone(PhoneNumberType type)
        {
            var rel = this.Account_PhoneNumber_Rel.FirstOrDefault(e => e.Type == (int)type);
            if (rel == null)
            {
                return null;
            }

            return rel.PhoneNumber;
        }
    }
}
