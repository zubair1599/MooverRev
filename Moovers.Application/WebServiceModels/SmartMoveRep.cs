using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Models;

namespace WebServiceModels
{
   public class SmartMoveRep
    {
        public string name { get; set; }
        public AddressRep address { get; set; }
        public LatLng address_latlng { get; set; }
        public AddressRep from { get; set; }
        public LatLng from_latlng { get; set; }
        public AddressRep to { get; set; }
        public LatLng to_latlng { get; set; }
        public DateTime move_date { get; set; }
        public string email { get; set; }
        public string telephone { get; set; } 
       
    }

    public class AddressRep
    {
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }

        public Address GetBusinessAddress()
        {
            return new Address(this.street, "", this.city, this.state, this.zip);
        }
    }
    public class LatLng {
        public string latitude { get; set; }
        public string longitude { get; set; }
    }
}
