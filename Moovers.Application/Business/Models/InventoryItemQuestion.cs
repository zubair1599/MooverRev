using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class InventoryItemQuestion
    {
        public object ToJsonObject()
        {
            return new {
                Time = this.Time,
                Weight = this.Weight,
                CubicFeet = this.CubicFeet,
                InventoryItemID = this.InventoryItemID,
                QuestionID = this.QuestionID,
                QuestionText = this.QuestionText,
                ShortName = this.ShortName,
                Options = this.InventoryItemQuestionOptions.OrderBy(i => i.Sort).Select(i => i.ToJsonObject())
            };
        }
    }
}
