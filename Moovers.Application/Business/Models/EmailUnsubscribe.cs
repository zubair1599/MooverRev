using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public partial class EmailUnsubscribe
    {
        public EmailUnsubscribe() { }

        public EmailUnsubscribe(string email, string ip)
        {
            this.Email = email;
            this.UnsubscribeIP = ip;
            this.DateUnsubscribed = DateTime.Now;
        }
    }
}
