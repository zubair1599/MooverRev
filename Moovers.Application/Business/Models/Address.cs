using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using Business.Repository.Models;

namespace Business.Models
{
    public partial class Address
    {
        public Address()
        {
            this.Created = DateTime.Now;
        }

        public Address(string street1, string street2, string city, string state, string zip)
            : this()
        {
            this.Street1 = street1;
            this.Street2 = street2;
            this.City = city;
            this.State = state;
            this.Zip = zip;
        }

        public ZipCode GetZip()
        {
            var zipRepo = new ZipCodeRepository();
            return zipRepo.Get(this.Zip) ?? zipRepo.GetByFirst3(this.Zip);
        }
         
        public LatLng GetLatLng()
        {
            if (this.IsVerified())
            {
                var verified = this.GetVerified();
                return new LatLng()
                {
                    Latitude = double.Parse(verified.metadata.latitude),
                    Longitude = double.Parse(verified.metadata.longitude)
                };
            }
            
            var repo = new ZipCodeRepository();
            var zip = repo.Get(this.Zip);
            if (zip != null)
            {
                return new LatLng()
                {
                    Latitude = (double)zip.Latitude,
                    Longitude = (double)zip.Longitude
                };
            }

            throw new Exception("Couldn't find zip or verified");
        }


        /// <summary>
        /// Get Drive time to "addr"
        /// </summary>
        public decimal GetTimeTo(Address addr)
        {
            if (this.SameAs(addr))
            {
                return 0;
            }

            try
            {
                return this.GetLatLng().GetTime(addr.GetLatLng());
            }
            catch (Exception e)
            {
                var repo = new ErrorRepository();
                repo.Log(e, "GOOGLE MAPS ERROR", new System.Collections.Specialized.NameValueCollection(), new System.Collections.Specialized.NameValueCollection());
                repo.Save();

                return this.GetLatLng().GetFlyoverTime(addr.GetLatLng());
            }
        }

        /// <summary>
        /// Get straight-line distance from Zip to Zip
        /// This is a very inexpensive/fast way to estimate distance between two addresses
        /// </summary>
        public decimal GetFlyoverDistanceTo(ZipCode zip)
        {
            var latlng = this.GetLatLng();
            return latlng.GetFlyoverDistance(zip.GetLatLng());
        }

        /// <summary>
        /// Get driving distance from Address to Address
        /// </summary>
        public decimal GetDistanceTo(Address addr)
        {
            if (this.SameAs(addr))
            {
                return 0;
            }

            try
            {
                return this.GetLatLng().GetDistance(addr.GetLatLng());
            }
            catch (Exception e)
            {
                var repo = new ErrorRepository();
                repo.Log(e, "GOOGLE MAPS ERROR", new System.Collections.Specialized.NameValueCollection(), new System.Collections.Specialized.NameValueCollection());
                repo.Save();

                return this.GetLatLng().GetFlyoverDistance(addr.GetLatLng());
            }
        }

        /// <summary>
        /// Display 5-digit zip (always ignore extension)
        /// </summary>
        public string DisplayZip()
        {
            return this.Zip.Substring(0, 5);
        }

        /// <summary>
        /// ex: Kansas City, MO
        /// </summary>
        public string DisplayCityState()
        {
            if (!String.IsNullOrEmpty(this.City) && !String.IsNullOrEmpty(this.State))
            {
                return this.City + ", " + this.State;
            }

            var repo = new ZipCodeRepository();
            var zip = repo.Get(this.Zip);

            if (zip != null) return zip.City + ", " + zip.State;
            return "";
        }

        /// <summary>
        /// ex: 3517 Enterprise Dr, Ste. D, Kansas City, MO 64129
        /// </summary>
        public string DisplayString() 
        {
            bool hasstreet = !String.IsNullOrWhiteSpace(this.Street1);
            bool hasCity = !String.IsNullOrWhiteSpace(this.City) && !String.IsNullOrWhiteSpace(this.State);
            
            if (!hasstreet && !hasCity)
            {
                return this.Zip;
            }
            
            if (!hasstreet)
            {
                return this.City + ", " + this.State + " " + this.Zip;
            }
            
            if (!String.IsNullOrEmpty(this.Street2))
            {
                return this.Street1 + " " + this.Street2 + ", " + this.City + ", " + this.State + " " + this.Zip;
            }

            return this.Street1 + ", " + this.City + ", " + this.State + " " + this.Zip;
        }

        /// <summary>
        /// ex: 3517 Enterprise Dr., Ste. D
        /// </summary>
        public string DisplayLine1()
        {
            if (!String.IsNullOrEmpty(this.Street1))
            {
                return (this.Street1 + " " + (this.Street2 ?? String.Empty)).Trim();
            }

            return this.City + ", " + this.State;
        }

        /// <summary>
        /// ex: 3517 Enterprise Dr Ste D Kansas City, MO 64129
        /// </summary>
        public string DisplaySmall()
        {
            var ret = String.Empty;
            if (!String.IsNullOrEmpty(this.Street1))
            {
                ret += (this.Street1 + " " + (this.Street2 ?? String.Empty)).Trim();
            }

            var smallZip = this.Zip.Length >= 5 ? this.Zip.Substring(0, 5) : this.Zip;
            return (ret + " " + this.City + ", " + this.State + " " + smallZip).Trim();
        }

        public bool IsVerified()
        {
            return this.VerifiedDate.HasValue;
        }

        public void SetVerified(CandidateAddress verifiedAddress)
        {
            this.Street1 = verifiedAddress.delivery_line_1;
            this.Street2 = String.Empty;
            this.City = verifiedAddress.components.city_name;
            this.State = verifiedAddress.components.state_abbreviation;
            this.Zip = verifiedAddress.components.zipcode + "-" + verifiedAddress.components.plus4_code;

            this.VerifiedLine1 = verifiedAddress.delivery_line_1;
            this.VerifiedLastLine = verifiedAddress.last_line;
            this.VerifiedBarCode = verifiedAddress.delivery_point_barcode;
            this.VerifiedAnalysis = Utility.LocalExtensions.SerializeToJson(verifiedAddress.analysis);
            this.VerifiedComponents = Utility.LocalExtensions.SerializeToJson(verifiedAddress.components);
            this.VerifiedMetaData = Utility.LocalExtensions.SerializeToJson(verifiedAddress.metadata);
            this.VerifiedDate = DateTime.Now;
        }

        public Address Duplicate()
        {
            var ret = new Address(this.Street1, this.Street2, this.City, this.State, this.Zip) {
                VerifiedLine1 = this.VerifiedLine1,
                VerifiedLastLine = this.VerifiedLastLine,
                VerifiedBarCode = this.VerifiedBarCode,
                VerifiedAnalysis = this.VerifiedAnalysis,
                VerifiedComponents = this.VerifiedComponents,
                VerifiedMetaData = this.VerifiedMetaData,
                VerifiedDate = this.VerifiedDate
            };
            return ret;
        }

        public void CopyTo(Address dest)
        {
            dest.Street1 = this.Street1;
            dest.Street2 = this.Street2;
            dest.City = this.City;
            dest.State = this.State;
            dest.Zip = this.Zip;
            dest.VerifiedLine1 = this.VerifiedLine1;
            dest.VerifiedLastLine = this.VerifiedLastLine;
            dest.VerifiedBarCode = this.VerifiedBarCode;
            dest.VerifiedAnalysis = this.VerifiedAnalysis;
            dest.VerifiedComponents = this.VerifiedComponents;
            dest.VerifiedMetaData = this.VerifiedMetaData;
            dest.VerifiedDate = this.VerifiedDate;
        }

        public bool SameAs(Address address2)
        {
            if (this.Street1 != address2.Street1)
            {
                return false;
            }

            if (this.Street2 != address2.Street2)
            {
                return false;
            }

            if (this.City != address2.City)
            {
                return false;
            }

            if (this.Zip != address2.Zip)
            {
                return false;
            }

            if (this.State != address2.State)
            {
                return false;
            }

            return true;
        }

        public object ToJsonObject()
        {
            return new 
            {
                AddressID = this.AddressID,
                Street1 = this.Street1,
                Street2 = this.Street2,
                City = this.City,
                State = this.State,
                Zip = this.Zip,
                Verified = this.IsVerified()
            };
        }

        public CandidateAddress GetVerified()
        {
            if (!this.VerifiedDate.HasValue)
            {
                return null;
            }

            return new CandidateAddress()
            {
                analysis = Newtonsoft.Json.JsonConvert.DeserializeObject<Analysis>(this.VerifiedAnalysis),
                components = Newtonsoft.Json.JsonConvert.DeserializeObject<Components>(this.VerifiedComponents),
                delivery_line_1 = this.VerifiedLine1,
                delivery_point_barcode = this.VerifiedBarCode,
                last_line = this.VerifiedLastLine,
                metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<Metadata>(this.VerifiedMetaData)
            };
        }
    }
}
