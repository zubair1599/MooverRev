using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public struct BoxRelJson
    {
        public Guid BoxTypeBoxID { get; set; }

        public string Name { get; set; }

        public int Count { get; set; }
    }
}
