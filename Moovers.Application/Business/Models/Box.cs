using System.Text;

namespace Business.Models
{
    public partial class Box
    {
        public object ToJsonObject()
        {
            return new {
                BoxTypeID = this.BoxTypeID,
                Name = this.Name
            };
        }
    }
}
