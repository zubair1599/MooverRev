using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class PasswordResetRepository : RepositoryBase<PasswordReset>
    {
        private const int ExpirationMinutes = 15;

        public override PasswordReset Get(Guid id)
        {
            var date = DateTime.Now.AddMinutes(-ExpirationMinutes);
            return db.PasswordResets.Where(i => i.DateRequested > date).SingleOrDefault(i => i.ResetID == id);
        }

        public PasswordReset Get(string key)
        {
            var date = DateTime.Now.AddMinutes(-ExpirationMinutes);
            return db.PasswordResets.Where(i => i.DateRequested > date).SingleOrDefault(k => k.ResetKey == key);
        }
    }
}