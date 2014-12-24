using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public struct AdditionalInfo
    {
        public Guid? OptionID { get; set; }

        public Guid QuestionID { get; set; }

        public string DisplayName { get; set; }
    }
}
