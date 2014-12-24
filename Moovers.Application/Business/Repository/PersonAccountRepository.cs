using System;
using System.Linq;
using Business.Models;
using Business.Utility;

namespace Business.Repository.Models
{
    public class PersonAccountRepository : RepositoryBase<PersonAccount>
    {
        public override PersonAccount Get(Guid id)
        {
            return (PersonAccount)db.Accounts.SingleOrDefault(a => a.AccountID == id);
        }

        public override void Add(PersonAccount item)
        {
            db.Accounts.AddObject(item);
        }

        public void UpdateFromForm(Guid franchiseID, PersonAccount account, ViewModels.PersonAccountModel model)
        {
            account.FirstName = model.Account.FirstName.Capitalize();
            account.LastName = model.Account.LastName.Capitalize();
            account.FranchiseID = franchiseID;
            model.SetupAccountProperties(account, this);
        }

        public void UpdateCustomerCredentials(ViewModels.PersonAccountModel model, PersonAccount account, object Userid)
        {
            var rel = new Account_Customer_Credentials { UserName = model.user_name,
                UserId = (Guid)Userid,
                AccountId = account.AccountID

            };
            db.Account_Customer_Credentials.AddObject(rel);
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.Account_Customer_Credentials.Where(i => i.UserId == userid);
            foreach (var item in items)
            {
                db.Account_Customer_Credentials.DeleteObject(item);
            }
        }
    }
}