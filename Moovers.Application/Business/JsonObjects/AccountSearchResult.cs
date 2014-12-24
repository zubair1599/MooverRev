using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public class AccountSearchResult
    {
        public Models.Account account { get; set; }

        public Models.PersonAccount person { get; set; }

        public Models.BusinessAccount business { get; set; }

        public string name { get; set; }

        public string sortname { get; set; }
    }
}
