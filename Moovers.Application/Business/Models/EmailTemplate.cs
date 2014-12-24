using System.Text;

namespace Business.Models
{
    public partial class EmailTemplate
    {
        public object ToJsonObject()
        {
            return new
            {
                this.Name,
                this.Subject,
                this.Text
            };
        }
    }
}
