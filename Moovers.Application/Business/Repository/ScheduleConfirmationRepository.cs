using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class ScheduleConfirmationRepository : RepositoryBase<ScheduleConfirmation>
    {
        private string GenerateLookup()
        {
            string key;
            ScheduleConfirmation existing;
            int lookups = 0;

            do
            {
                key = Utility.General.RandomString(8);
                existing = this.Get(key);
                lookups++;
            } while (existing != null && lookups < 20);
            
            return key;
        }

        private readonly Func<MooversCRMEntities, Guid, ScheduleConfirmation> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, ScheduleConfirmation>(
            (db, id) => db.ScheduleConfirmations.SingleOrDefault(i => i.ConfirmationID == id)
            );

        private readonly Func<MooversCRMEntities, string, ScheduleConfirmation> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, ScheduleConfirmation>(
            (db, key) => db.ScheduleConfirmations.SingleOrDefault(i => i.Key == key)
            );

        public override ScheduleConfirmation Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public ScheduleConfirmation Get(string key)
        {
            return CompiledGetByLookup(db, key);
        }

        public ScheduleConfirmation CreateNew()
        {
            var ret = new ScheduleConfirmation {
                Key = this.GenerateLookup()
            };
            return ret;
        }
    }
}