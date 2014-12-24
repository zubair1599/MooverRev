using System;
using System.Dynamic;
using System.Linq.Expressions;
using System.Text;
using LinqKit;
using System.Diagnostics.CodeAnalysis;
using Business;
using System.ComponentModel;
using Business.Utility;

namespace Business.Models
{
    public enum BusinessTypes
    {
        Commercial,
        Government,

        [Description("Non Profit")]
        Non_Profit,
        Vendor,
        Competitor
    }

    public partial class BusinessAccount
    {
        private static readonly Expression<Func<BusinessAccount, string>> NameSelector = (account) => account.Name;

        public BusinessTypes Type
        {
            get { return (BusinessTypes)this.BusinessType; }
            set { this.BusinessType = (int)value; }
        }

        public BusinessAccount()
        {
            this.Created = DateTime.Now;
        }

        public override string DisplayNameLastFirst
        {
            get { return BusinessAccount.NameSelector.Invoke(this); }
        }

        public override string DisplayName
        {
            get { return BusinessAccount.NameSelector.Invoke(this); }
        }

        private dynamic GetCustomJson()
        {
            dynamic ret = new ExpandoObject();
            ret.Name = this.Name;
            ret.Lookup = this.Lookup;
            ret.BusinessType = this.Type.ToString();
            ret.ViewBusinessType = this.Type.GetDescription();
            return ret;
        }

        public override dynamic ToMiniJsonObject()
        {
            var baseObj = base.ToMiniJsonObject();
            var thisObj = this.GetCustomJson();
            return Utility.General.Combine(baseObj, thisObj);
        }

        public override dynamic ToJsonObject()
        {
            dynamic thisObj = this.GetCustomJson();
            var baseObj = base.ToJsonObject();
            return Utility.General.Combine(baseObj, thisObj);
        }
    }
}
