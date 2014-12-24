using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class QuoteSurveyRepository : RepositoryBase<QuoteSurvey>
    {
        private static readonly Func<MooversCRMEntities, Guid, DateTime, DateTime, IEnumerable<QuoteSurvey>> CompiledGetBetween = CompiledQuery.Compile(
            (MooversCRMEntities db, Guid franchiseID, DateTime start, DateTime end) => 
                (from survey in db.QuoteSurveys
                    where 
                        survey.Quote.FranchiseID == franchiseID
                        && survey.Date >= start && survey.Date <= end 
                        && !survey.IsCancelled
                    select survey)
            );

        public override QuoteSurvey Get(Guid id)
        {
            return db.QuoteSurveys.SingleOrDefault(i => i.SurveyID == id);
        }

        public IEnumerable<QuoteSurvey> GetBetween(Guid franchiseID, DateTime start, DateTime end)
        {
            return CompiledGetBetween(db, franchiseID, start, end);
        }

        public IEnumerable<QuoteSurvey> GetForDay(Guid franchiseID, DateTime date)
        {
            var mindate = date.Date;
            var maxdate = date.Date.AddDays(1).AddMilliseconds(-1);
            return this.GetBetween(franchiseID, mindate, maxdate);
        }
    }
}