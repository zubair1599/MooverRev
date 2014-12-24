using System;
using System.Collections.Generic;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class BusinessAccountRepository : RepositoryBase<BusinessAccount>
    {
        public override BusinessAccount Get(Guid id)
        {
            return (BusinessAccount)db.Accounts.SingleOrDefault(a => a.AccountID == id);
        }

        public IEnumerable<BusinessAccount> GetAll()
        {
            return db.Accounts.OfType<BusinessAccount>().OrderBy(a => a.Name);
        }

        public override void Add(BusinessAccount item)
        {
            this.db.Accounts.AddObject(item);
        }

        public BusinessAccount UpdateFromForm(Guid franchiseID, BusinessAccount account, ViewModels.BusinessAccountModel model)
        {
            account.Name = model.Account.Name;
            account.Type = model.Account.Type;
            account.FranchiseID = franchiseID;
            model.SetupAccountProperties(account, this);
            return account;
        }
    }
}