using System;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class Account_CreditCardRepository : RepositoryBase<Account_CreditCard>
    {
        public override Account_CreditCard Get(Guid id)
        {
            return db.Account_CreditCard.SingleOrDefault(i => i.CreditCardID == id);
        }
    }
}
