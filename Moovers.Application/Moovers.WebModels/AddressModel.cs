using System;
using Business.Models;

namespace Moovers.WebModels
{
    public class AddressModel
    {
        private string _class = "address";

        private int _tabindex = -1;

        public int TabIndex
        {
            get { return _tabindex; }
            set { _tabindex = value; }
        }

        public string Class
        {
            get { return _class; }
            set { _class = value; }
        }

        public Business.Models.Address Address { get; set; }

        public Business.Models.AddressType Type { get; set; }

        private bool _required = true;

        #region Form Elements

        public string street1 { get; set; }

        public string street2 { get; set; }

        public string city { get; set; }

        public string state { get; set; }

        public string zip { get; set; }

        #endregion

        public bool Required
        {
            get { return _required; }
            set { _required = value; }
        }

        public AddressModel(Business.Models.AddressType type, bool required)
        {
            this.Address = new Business.Models.Address();
            this.Type = type;
            this.Required = required;
        }

        public Business.Models.Address GetAddress()
        {
            return new Business.Models.Address(this.street1, this.street2, this.city, this.state, this.zip);
        }

        public AddressModel() { }

        public AddressModel(Business.Models.AddressType type, Business.Models.Address address)
            : this(type, true)
        {
            this.Address = address;
        }

        public AddressModel(Business.Models.Address address, bool required, string classname = "")
            : this(AddressType.Billing, required)
        {
            this.Address = address;
            if (!String.IsNullOrEmpty(classname))
            {
                this.Class = classname;
            }
        }
    }
}