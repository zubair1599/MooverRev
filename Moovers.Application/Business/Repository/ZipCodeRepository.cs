using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class ZipCodeRepository : RepositoryBase<ZipCode>, Interfaces.IZipcodeRepository
    {
        private Func<MooversCRMEntities, Guid, ZipCode> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, ZipCode>(
            (db, id) => db.ZipCodes.SingleOrDefault(i => i.ZipCodeID == id)
            );

        private Func<MooversCRMEntities, string, ZipCode> CompiledGetByText = CompiledQuery.Compile<MooversCRMEntities, string, ZipCode>(
            (db, id) => db.ZipCodes.FirstOrDefault(i => i.Zip == id)
            );

        public override ZipCode Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public ZipCode Get(string zip)
        {
            if (zip.Length >= 5)
            {
                zip = zip.Substring(0, 5);
                return CompiledGetByText(db, zip);
            }

            return null;
        }

        /// <summary>
        /// Emergency zip code if we can't find what the customer entered on a web quote -- never returns null.
        /// </summary>
        /// <returns></returns>
        public ZipCode GetDefault()
        {
            return Get("64129");
        }

        public ZipCode GetByFirst3(string zip)
        {
            zip = zip.Substring(0, 3);
            return (from z in db.ZipCodes
                where z.Zip.Substring(0, 3) == zip
                select z).FirstOrDefault();
        }
    }
}