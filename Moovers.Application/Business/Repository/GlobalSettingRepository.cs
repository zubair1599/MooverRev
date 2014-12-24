using System;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Utility;

namespace Business.Repository.Models
{
    public class GlobalSettingRepository : RepositoryBase<GlobalSetting>
    {
        private Guid FranchiseID { get; set; }

        public GlobalSettingRepository(Guid franchiseID)
        {
            this.FranchiseID = franchiseID;
        }

        public override GlobalSetting Get(Guid id)
        {
            return db.GlobalSettings.SingleOrDefault(i => i.SettingID == id);
        }

        public void SetValue(SettingTypes key, string value)
        {
            var cacheRepo = new CacheRepository();
            cacheRepo.Clear();

            var desc = key.GetDescription();

            var item = (from i in db.GlobalSettings
                where i.Key == desc
                      && i.FranchiseID == this.FranchiseID
                select i).Single();
            item.Value = value;
        }

        public T GetValue<T>(SettingTypes key)
        {
            var cacheRepo = new CacheRepository();
            var desc = key.GetDescription();
            string cachekey = "GLOBALSETTING - " + desc + " - " + this.FranchiseID.ToString();
            if (!cacheRepo.Contains(cachekey))
            {
                var item = (from i in db.GlobalSettings
                    where i.Key == desc
                          && i.FranchiseID == this.FranchiseID
                    select i).SingleOrDefault();

                var value = Convert.ChangeType(item.Value, typeof(T));
                cacheRepo.Store(cachekey, value);
            }

            return cacheRepo.Get<T>(cachekey);
        }
    }
}