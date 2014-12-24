namespace Business.Models
{
    public partial class InventoryItemQuestionOption
    {
        public object ToJsonObject()
        {
            return new {
                Time = this.Time,
                Weight = this.Weight,
                CubicFeet = this.CubicFeet,
                QuestionID = this.QuestionID,
                OptionID = this.OptionID,
                Selected = this.Selected,
                Option = this.Option,
                Sort = this.Sort
            };
        }
    }
}