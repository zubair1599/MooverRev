using Business.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class EmployeeAuthorization
    {
        public EmployeeAuth_Session CurAuthSession { get; set; }
        public bool Authenticate(string password)
        {
            var toTest = Security.HashString(password, this.PasswordSalt);
            return toTest.Equals(this.PasswordHash);
        }

        public void SetPassword(string password)
        {
            this.PasswordSalt = Guid.NewGuid();
            var hash = Security.HashString(password, this.PasswordSalt);
            this.PasswordHash = hash;
        }
    }
}
