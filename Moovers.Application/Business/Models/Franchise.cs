using System;
using System.Text;
using System.Web;

namespace Business.Models
{
    public partial class Franchise
    {
        public string GetIconUrl()
        {
            if (String.IsNullOrEmpty(this.Icon))
            {
                return "";
            }

            return VirtualPathUtility.ToAbsolute(this.Icon);
        }

        public string DisplayNumber()
        {
            if (String.IsNullOrEmpty(this.PhoneNumber) || this.PhoneNumber.Length < 10)
            {
                return String.Empty;
            }

            return new PhoneNumber(this.PhoneNumber).DisplayString();
        }
    }
}
