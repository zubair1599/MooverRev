using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.ViewModels
{
    public class EmailModel
    {
        public string To { get; set; }

        public string From { get; set; }

        public Models.Account Account { get; set; }

        public Models.aspnet_Users_Profile AccountManager { get; set; }

        public Models.Franchise Franchise { get; set; }

        public string Message { get; set; }
    }
}
