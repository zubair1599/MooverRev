using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.Models
{
    public partial class EmailAddress
    {
        public EmailAddress(){ }

        public EmailAddress(string email)
            : this()
        {
            this.Email = email;
        }

        public object ToJsonObject()
        {
            return this.Email;
        }

        public static bool IsEmailAddress(string address)
        {
            if (String.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            try
            {
                // this constructor throws a Format exception if "address" isn't an email address
                new System.Net.Mail.MailAddress(address);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
