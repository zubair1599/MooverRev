using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Business.Models
{
    public enum UserType
    {
        [Description("Customer")]
        Customer,
        [Description("CRMUSER")]
        CRM
    }
    public partial class aspnet_Users_Profile
    {
        public string DisplayName()
        {
            return this.FirstName + " " + this.LastName;
        }

        public string DisplayNumber()
        {
            return new PhoneNumber(this.Phone).DisplayString();
        }
    }
}
