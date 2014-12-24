using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.Models
{
    public partial class PhoneNumber
    {
        private string AreaCode
        {
            get 
            {
                if (this.Number.Length >= 10)
                {
                    return this.Number.Substring(0, 3); 
                }

                return String.Empty;
            }
        }

        private string Prefix
        {
            get 
            {
                if (this.Number.Length >= 10)
                {
                    return this.Number.Substring(3, 3);
                }
                
                if (this.Number.Length == 7)
                {
                    return this.Number.Substring(0, 3);
                }


                return String.Empty;
            }
        }

        private string Suffix
        {
            get 
            {
                if (this.Number.Length >= 10)
                {
                    return this.Number.Substring(6, 4); 
                }

                if (this.Number.Length == 7)
                {
                    return this.Number.Substring(3);
                }

                return String.Empty;
            }
        }

        public PhoneNumber()
        {
            this.Created = DateTime.Now;
        }

        public PhoneNumber(string number) : this()
        {
            number = number ?? String.Empty;
            number = Regex.Replace(number, @"[^\d]+", String.Empty);

            this.Number = number;
        }

        public string DisplayString()
        {
            string displayNumber = String.IsNullOrEmpty(this.AreaCode) ? 
                    String.Format("{0}-{1}", this.Prefix, this.Suffix) : 
                    String.Format("({0}) {1}-{2}", this.AreaCode, this.Prefix, this.Suffix);

            if (!String.IsNullOrEmpty(this.Extension))
            {
                displayNumber = displayNumber + " - " + this.Extension;
            }

            return displayNumber;
        }

        public bool IsValid()
        {
            if (String.IsNullOrEmpty(this.Number))
            {
                return false;
            }

            if (this.Number.Length != 10)
            {
                return false;
            }

            return true;
        }

        public object ToJsonObject()
        {
            return new
            {
                Number,
                Extension,
                Display = this.DisplayString()
            };
        }
    }
}
