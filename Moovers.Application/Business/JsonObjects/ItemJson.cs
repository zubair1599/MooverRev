using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public struct ItemJson
    {
        public Guid ItemID { get; set; }

        public int? KeyCode { get; set; }

        public string Name { get; set; }

        public decimal Weight { get; set; }

        public decimal CubicFeet { get; set; }

        public int MoversRequired { get; set; }

        public bool IsBox { get; set; }

        public bool IsSpecialItem { get; set; }

        public decimal? CustomTime { get; set; }

        public IEnumerable<string> Aliases { get; set; }

        public IEnumerable<object> AdditionalQuestions { get; set; }

        public IEnumerable<BoxRelJson> Boxes { get; set; }
    }
}
