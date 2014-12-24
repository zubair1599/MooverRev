namespace Business.Models
{
    public partial class Crew_Employee_Rel
    {
        public object ToJsonObject()
        {
            return new {
                Employee = this.Employee.ToJsonObject(),
                IsDriver = this.IsDriver
            };
        }
    }
}