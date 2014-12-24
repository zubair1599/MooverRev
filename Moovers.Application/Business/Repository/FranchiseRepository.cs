using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class FranchiseRepository : RepositoryBase<Franchise>, Interfaces.IFranchiseRepository
    {
        private static readonly Func<MooversCRMEntities, Guid, Franchise> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Franchise>(
            (db, id) => db.Franchises.SingleOrDefault(i => i.FranchiseID == id)
            );

        public Franchise GetStorage()
        {
            return db.Franchises.SingleOrDefault(i => i.HasStorage);
        }

        public override Franchise Get(Guid id)
        {
            var cacheRepo = new CacheRepository();
            var key = "franchise" + id.ToString();
            if (!cacheRepo.Contains(key))
            {
                cacheRepo.Store(key, CompiledGetByID(db, id));
            }

            return cacheRepo.Get<Franchise>(key);
        }

        public IEnumerable<Franchise> GetAll()
        {
            return db.Franchises.OrderBy(i => i.Name);
        }

        public Franchise GetAssignedTo(ZipCode zip)
        {
            // each franchise has a list of zip codes they are assigned to
            // before determining driving distance, first we check to see if the zip is in a franchises territory
            var byTerritory = (from f in db.FranchiseTerritories
                where f.ZipCode == zip.Zip
                select f).FirstOrDefault();

            if (byTerritory != null)
            {
                return byTerritory.Franchise;
            }

            // if the zip code isn't assigned to a franchise, get driving distance
            return this.GetClosestTo(zip);
        }

        public Franchise GetClosestTo(ZipCode zip)
        {
            var franchises = db.Franchises.Include("Address").ToList();

            decimal distance = decimal.MaxValue;
            Franchise closest = default(Franchise);
            foreach (var f in franchises)
            {
                var thisDist = f.Address.GetFlyoverDistanceTo(zip);
                if (thisDist < distance)
                {
                    distance = thisDist;
                    closest = f;
                }
            }

            return closest;
        }

        public Guid GetSingleFranchise()
        {
            return db.Franchises.FirstOrDefault().FranchiseID;
        }
        public Guid GetUserFranchise(Guid userId)
        {
            return db.aspnet_Users.FirstOrDefault(u => u.UserId == userId).Franchise_aspnetUser.First().FranchiseID;
        }
    }
}