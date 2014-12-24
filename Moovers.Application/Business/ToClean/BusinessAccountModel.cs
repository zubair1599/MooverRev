using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Business.ViewModels
{
    public class BusinessAccountModel : AccountModelBase<Models.BusinessAccount>
    {
        public override Models.BusinessAccount Account { get; set; }
    }
}