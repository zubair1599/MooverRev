using System;
using System.Text;

namespace Business.Models
{
    public partial class StorageComment
    {
        public bool IsEditable()
        {
            return this.Date.AddHours(1) > DateTime.Now;
        }

        public object ToJsonObject()
        {
            return new {
                CommentID = this.StorageCommentID,
                Date = this.Date,
                Text = this.Text,
                UserName = this.aspnet_Users.UserName,
                IsEditable = this.IsEditable()
            };
        }
    }
}
