using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Business.Repository;

namespace Business.ViewModels
{
    public abstract class AccountModelBase<T>
        where T : Models.Account
    {
        public abstract T Account { get; set; }

        public Models.Address MailingAddress { get; set; }

        public Models.Address BillingAddress { get; set; }

        public Models.PhoneNumber PrimaryPhone { get; set; }

        public Models.PhoneNumber SecondaryPhone { get; set; }

        public Models.PhoneNumber FaxPhone { get; set; }

        public Models.EmailAddress PrimaryEmail { get; set; }

        public Models.EmailAddress SecondaryEmail { get; set; }

        public string verified { get; set; }

        public CandidateAddress GetVerifiedAddress()
        {
            if (String.IsNullOrEmpty(verified))
            {
                return null;
            }

            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<CandidateAddress>(this.verified);
            if (String.IsNullOrEmpty(obj.delivery_line_1))
            {
                return null;
            }

            return obj;
        }

        private bool _copyMailing = true;

        public bool CopyMailing
        {
            get { return _copyMailing; }
            set { _copyMailing = value; }
        }

        private static Dictionary<AddressType, Address> GetAddressesFromForms(FormCollection coll)
        {
            try
            {
                var ret = new Dictionary<AddressType, Address>();
                var types = coll["addr_type"].Split('|');
                var street1s = coll["street1"].Split('|');
                var street2s = coll["street2"].Split('|');
                var cities = coll["city"].Split('|');
                var states = coll["state"].Split('|');
                var zips = coll["zip"].Split('|');

                for (var i = 0; i < types.Length; i++)
                {
                    var type = (AddressType)Enum.Parse(typeof(AddressType), types[i]);
                    var address = new Address(street1s[i], street2s[i], cities[i], states[i], zips[i]);
                    if (type == AddressType.Billing)
                    {
                        ret.Add(AddressType.Billing, address);
                    }

                    if (type == AddressType.Mailing)
                    {
                        ret.Add(AddressType.Mailing, address);
                    }
                }

                return ret;
            }
            catch (Exception e)
            {
                throw new AddressParseException(e.Message, e);
            }
        }

        public void UpdateAddresses(Account account, FormCollection coll, bool currentMailing)
        {
            try
            {
                var addresses = GetAddressesFromForms(coll);
                if (currentMailing && account != null) {
                    this.MailingAddress = account.GetAddress(Models.AddressType.Mailing);
                }
                else {
                    this.MailingAddress = addresses[Models.AddressType.Mailing];
                }

                this.BillingAddress = this.CopyMailing ? this.MailingAddress.Duplicate() : addresses[Models.AddressType.Billing];

                var verifiedAddress = this.GetVerifiedAddress();
                if (verifiedAddress != null)
                {
                    this.MailingAddress.SetVerified(verifiedAddress);
                    if (this.CopyMailing)
                    {
                        this.BillingAddress.SetVerified(verifiedAddress);
                    }
                }
            }
            catch (Exception)
            {
                this.MailingAddress = null;
                this.BillingAddress = null;
            }
        }

        public void SetupAccountProperties(T account, RepositoryBase repo)
        {
            var accountRepo = new AccountRepository(repo.db);

            account.SetAddress(this.BillingAddress, Models.AddressType.Billing);
            account.SetAddress(this.MailingAddress, Models.AddressType.Mailing);
            account.SetPhoneNumber(this.PrimaryPhone, Models.PhoneNumberType.Primary);

            if (this.PrimaryEmail != null && !String.IsNullOrEmpty(this.PrimaryEmail.Email))
            {
                account.SetEmail(this.PrimaryEmail, Models.EmailAddressType.Primary);
            }
            else
            {
                accountRepo.RemoveEmail(account, Models.EmailAddressType.Primary);
            }

            if (this.SecondaryEmail != null && !String.IsNullOrEmpty(this.SecondaryEmail.Email))
            {
                account.SetEmail(this.SecondaryEmail, Models.EmailAddressType.Secondary);
            }
            else
            {
                accountRepo.RemoveEmail(account, Models.EmailAddressType.Secondary);
            }

            if (this.SecondaryPhone != null && !String.IsNullOrEmpty(this.SecondaryPhone.Number))
            {
                account.SetPhoneNumber(this.SecondaryPhone, Models.PhoneNumberType.Secondary);
            }
            else
            {
                accountRepo.RemovePhone(account, Models.PhoneNumberType.Secondary);
            }

            if (this.FaxPhone != null && !String.IsNullOrEmpty(this.FaxPhone.Number))
            {
                account.SetPhoneNumber(this.FaxPhone, Models.PhoneNumberType.Fax);
            }
            else
            {
                accountRepo.RemovePhone(account, Models.PhoneNumberType.Fax);
            }
        }
    }
}