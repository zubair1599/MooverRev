using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Utility;

namespace Business.JsonObjects
{
    public struct LeadJson
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string CurrentZip { get; set; }

        public string CurrentCity { get; set; }

        public string CurrentState { get; set; }

        public string HomePhone { get; set; }

        public string WorkPhone { get; set; }

        public string MobilePhone { get; set; }

        public string Email { get; set; }

        public string ContactPreference { get; set; }

        public DateTime? MoveDate { get; set; }

        public string Weight { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }

        public string Comments { get; set; }

        public DateTime? DateSubmitted { get; set; }

        public Models.Account GetAccount()
        {
            var account = new Models.PersonAccount();

            var name = (this.Name ?? String.Empty).Trim();
            if (name.Contains(" "))
            {
                account.FirstName = name.Substring(0, name.IndexOf(" ")).Trim();
                account.LastName = name.Substring(name.IndexOf(" ")).Trim();                
            }
            else
            {
                account.FirstName = String.Empty;
                account.LastName = name;
            }

            account.FirstName = account.FirstName.Capitalize();
            account.LastName = account.LastName.Capitalize();
            account.SetAddress(new Models.Address() { Zip = this.CurrentZip, City = this.CurrentCity, State = Utility.General.GetStateAbbreviation(this.CurrentState) ?? "MO" }, Models.AddressType.Billing);
            account.SetAddress(new Models.Address() { Zip = this.CurrentZip, City = this.CurrentCity, State = Utility.General.GetStateAbbreviation(this.CurrentState) ?? "MO" }, Models.AddressType.Mailing);

            if (!String.IsNullOrWhiteSpace(this.HomePhone) && this.HomePhone.Length >= 10)
            {
                account.SetPhoneNumber(new Models.PhoneNumber(this.HomePhone), Models.PhoneNumberType.Primary);
            }

            if (!String.IsNullOrWhiteSpace(this.MobilePhone) && this.MobilePhone.Length >= 10)
            {
                account.SetPhoneNumber(new Models.PhoneNumber(this.MobilePhone), Models.PhoneNumberType.Secondary);
            }
            else if (!String.IsNullOrWhiteSpace(this.WorkPhone) && this.WorkPhone.Length >= 10)
            {
                account.SetPhoneNumber(new Models.PhoneNumber(this.WorkPhone), Models.PhoneNumberType.Secondary);
            }

            if (!String.IsNullOrWhiteSpace(this.Email))
            {
                account.SetEmail(new Models.EmailAddress() { Email = this.Email }, Models.EmailAddressType.Primary);
            }

            return account;
        }
    }
}
