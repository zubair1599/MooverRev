// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="AccountRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using Business.Models;

    public class AccountRepresentation
    {
        public AccountRepresentation(Account account)
        {
            if (account != null)
            {
                first_name = account.PersonAccount != null ? account.PersonAccount.FirstName : account.BusinessAccount.DisplayName;
                last_name = account.PersonAccount != null ? account.PersonAccount.LastName : string.Empty;
                look_up = account.Lookup;
                customer_id = account.AccountID.ToString();
                EmailAddress emailAddress = account.GetEmail(EmailAddressType.Primary) ?? account.GetEmail(EmailAddressType.Secondary);
                phone1 = account.GetPhone(PhoneNumberType.Primary) != null ? account.GetPhone(PhoneNumberType.Primary).DisplayString() : string.Empty;
                phone2 = account.GetPhone(PhoneNumberType.Secondary) != null ? account.GetPhone(PhoneNumberType.Secondary).DisplayString() : string.Empty;

                if (emailAddress != null)
                {
                    email = emailAddress.Email;
                }
            }
        }

        public AccountRepresentation()
        {
        }

        public string customer_id { get; set; }

        public string look_up { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string phone1 { get; set; }

        public string phone2 { get; set; }

        public string email { get; set; }
    }
}