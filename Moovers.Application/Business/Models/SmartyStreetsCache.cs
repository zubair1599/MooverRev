using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public partial class SmartyStreetsCache
    {
        public SmartyStreetsCache()
        {
            this.DateAdded = DateTime.Now;
        }
        
        public SmartyStreetsCache(Address address, string response)
            : this()
        {
            this.Street = (address.Street1 + " " + (address.Street2 ?? String.Empty)).Trim();
            this.City = address.City;
            this.State = address.State;
            this.ZipCode = address.Zip;
            this.Response = response;
        }
    }
}
