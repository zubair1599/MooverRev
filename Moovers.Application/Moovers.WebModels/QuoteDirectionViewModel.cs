using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Models;

namespace Moovers.WebModels
{
    public class QuoteDirectionViewModel
    {
        public string Lookup { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public DateTime checkin_time { get; set; }

        public QuoteDirectionViewModel(List<QuoteMapDirection> list)
        {
            
        }
    }
}
