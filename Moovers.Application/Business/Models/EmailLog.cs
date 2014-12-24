using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Models
{
    public partial class EmailLog
    {
        public void AddFile(Guid fileid)
        {
            var rel = new EmailLog_File_Rel {
                FileID = fileid
            };
            this.EmailLog_File_Rel.Add(rel);
        }
    }
}
