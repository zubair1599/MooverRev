using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class EmailUnsubscribeRepository : RepositoryBase<EmailUnsubscribe>
    {
        private static readonly Func<MooversCRMEntities, Guid, EmailUnsubscribe> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, EmailUnsubscribe>(
            (db, id) => db.EmailUnsubscribes.SingleOrDefault(i => i.UnsubscribeID == id)
            );

        private static readonly Func<MooversCRMEntities, string, EmailUnsubscribe> CompiledGetByEmail = CompiledQuery.Compile<MooversCRMEntities, string, EmailUnsubscribe>(
            (db, email) => db.EmailUnsubscribes.SingleOrDefault(i => i.Email == email)
            );

        public override EmailUnsubscribe Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public EmailUnsubscribe Get(string email)
        {
            return CompiledGetByEmail(db, email);
        }

        public bool Exists(string email)
        {
            return this.Get(email) != default(EmailUnsubscribe);
        }
    }
}