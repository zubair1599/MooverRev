using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public partial class QuoteComment
    {
        public QuoteComment()
        {
            this.Date = DateTime.Now;
        }

        public bool IsEditable()
        {
            return this.Date.AddHours(1) > DateTime.Now;
        }

        public object ToJsonObject()
        {
            return new
            {
                CommentID = this.QuoteCommentID,
                Date = this.Date,
                Text = this.Text,
                UserName = this.aspnet_Users.UserName,
                IsEditable = this.IsEditable()
            };
        }
    }
}