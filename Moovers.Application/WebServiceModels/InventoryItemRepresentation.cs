using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServiceModels
{
    public class InventoryItemRepresentation
    {
        public Guid Item_id { get; set; }
        public int key_code { get; set; }
        public string name { get; set; }
        public decimal weight { get; set; }
        public decimal cubic_feet { get; set; }
        public int moovers_required { get; set; }
        public bool is_box { get; set; }
        public bool is_special_item { get; set; }
        public decimal? custom_time { get; set; }
        public decimal? liability_cost { get; set; }
        public List<WebServiceModels.Addendum> addendums { get; set; }

    }
}
