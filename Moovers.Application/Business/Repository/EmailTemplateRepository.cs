using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class EmailTemplateRepository : RepositoryBase<EmailTemplate>
    {
        public override EmailTemplate Get(Guid id)
        {
            return db.EmailTemplates.SingleOrDefault(i => i.TemplateID == id);
        }

        public IEnumerable<EmailTemplate> GetAll()
        {
            return db.EmailTemplates.OrderBy(i => i.Name);
        }
    }
}