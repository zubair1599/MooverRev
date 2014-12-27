using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MooversCRM.Attributes
{
    public class MenuDescriptionAttribute : Attribute
    {
        public string MenuName { get; set; }

        public MenuDescriptionAttribute(string name)
        {
            this.MenuName = name;
        }
    }
}