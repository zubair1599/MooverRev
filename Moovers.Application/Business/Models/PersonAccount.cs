using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data.Linq;
using Business;
using System.Dynamic;
using LinqKit;
using System.Web.Mvc;

namespace Business.Models
{
    public partial class PersonAccount : Account
    {
        public override string DisplayNameLastFirst
        {
            get { return this.LastName + ", " + this.FirstName; }
        }

        public override string DisplayName
        {
            get
            {
                if (String.IsNullOrWhiteSpace(this.FirstName) && String.IsNullOrWhiteSpace(this.LastName))
                {
                    return "NO NAME";
                }

                return this.FirstName + " " + this.LastName;
            }
        }

        public PersonAccount()
        {
            this.Created = DateTime.Now;
        }

        private dynamic GetCustomJson()
        {
            dynamic ret = new ExpandoObject();
            ret.FirstName = this.FirstName;
            ret.LastName = this.LastName;
            ret.Type = this.DisplayType();
            return ret;
        }

        public override dynamic ToMiniJsonObject()
        {
            var thisObj = this.GetCustomJson();
            var baseObj = base.ToMiniJsonObject();
            return Utility.General.Combine(baseObj, thisObj);
        }

        public override dynamic ToJsonObject()
        {
            var baseObj = base.ToJsonObject();
            var thisObj = this.GetCustomJson();
            return Utility.General.Combine(baseObj, thisObj);
        }
    }
}
