using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public class StorageVaultJson
    {
        public string Lookup { get; set; }

        public string Zone { get; set; }

        public string Row { get; set; }

        public string Shelf { get; set; }

        public string CurrentWorkOrder { get; set; }
    }
}
