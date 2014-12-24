using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Business.ViewModels;

namespace Business.ViewModels
{
    public class PersonAccountModel : AccountModelBase<Models.PersonAccount>
    {
        public override Models.PersonAccount Account { get; set; }
        public string password { get; set; }
        public string user_name { get; set; }
        public string confirmpassword { get; set; }
    }
}