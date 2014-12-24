using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models
{
    public partial class Franchise_aspnetUser
    {
    }

    public class Franchise_aspnet_UserRepository : RepositoryBase<Franchise_aspnetUser>
    {
        public override Franchise_aspnetUser Get(Guid id)
        {
            return db.Franchise_aspnetUser.SingleOrDefault(i => i.RelationshipID == id);
        }

        public IEnumerable<Franchise_aspnetUser> GetByUserId(Guid userid)
        {
            return db.Franchise_aspnetUser.Where(i => i.UserID == userid).ToList();
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.Franchise_aspnetUser.Where(i => i.UserID == userid);
            foreach (var item in items)
            {
                db.Franchise_aspnetUser.DeleteObject(item);
            }
        }

        public List<Guid> GetAllUserFranchises(Guid userid)
        {
            return db.Franchise_aspnetUser.Where(i => i.UserID == userid).Select(model=>model.FranchiseID).ToList();
        }
    }
}
