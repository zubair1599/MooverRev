using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class CachedMapResponseRepository : RepositoryBase<CachedMapResponse>
    {
        private static readonly Func<MooversCRMEntities, Guid, CachedMapResponse> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, CachedMapResponse>(
            (db, id) => db.CachedMapResponses.SingleOrDefault(i => i.ResponseID == id)
            );

        private static readonly Func<MooversCRMEntities, string, string, CachedMapResponse> CompiledGetByCoordinates = CompiledQuery.Compile<MooversCRMEntities, string, string, CachedMapResponse>(
            (db, str1, str2) => (db.CachedMapResponses.Where(i => i.Coordinate1 == str1 && i.Coordinate2 == str2).OrderByDescending(i => i.Date)).FirstOrDefault()
            );

        public override CachedMapResponse Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public CachedMapResponse GetByCoordinates(LatLng coord1, LatLng coord2)
        {
            string str1 = coord1.ToString();
            string str2 = coord2.ToString();
            return CompiledGetByCoordinates(db, str1, str2);
        }
    }
}