using System;
using System.Data.Objects;
using System.Linq;
using Business.Models;
using Business.Repository.Models;

namespace Business.Repository
{
    public class LeadRepository : RepositoryBase<Lead>
    {
        private static Func<MooversCRMEntities, Guid?, int> CompiledGetUnreadCount = CompiledQuery.Compile<MooversCRMEntities, Guid?, int>(
            (db, franchiseid) => db.Leads.Count(i => !i.IsArchived && (!franchiseid.HasValue || i.FranchiseID == franchiseid))
            );

        private static Func<MooversCRMEntities, Guid, Lead> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Lead>(
            (db, id) => db.Leads.SingleOrDefault(i => i.LeadID == id)
            );

        public override Lead Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public IQueryable<Lead> GetAll(Guid? franchiseID)
        {
            return (from l in db.Leads
                where !l.IsArchived
                      && (l.FranchiseID == franchiseID || !franchiseID.HasValue)
                orderby l.AddedDate descending
                select l);
        }
        public IQueryable<Lead> GetbyAccount(Guid? accountID)
        {
            return (from l in db.Leads
                where  l.AccountID == accountID
                orderby l.AddedDate descending
                select l);
        }
        public IQueryable<Lead> GetWithEmailsSent(DateTime after, DateTime before, int emailsSent)
        {
            return (from l in db.Leads
                where l.EmailsSent == emailsSent
                      && l.AddedDate >= after
                      && l.AddedDate <= before
                      && !l.IsArchived
                select l);
        }
        public int GetUnreadCount(Guid? franchiseID = null)
        {
            return CompiledGetUnreadCount(db, franchiseID);
        }

        public IQueryable<Lead> GetTodayLeads()
        {
           return (from l in db.Leads
                         where l.AddedDate.Day == DateTime.Now.Day && l.AddedDate.Month == DateTime.Now.Month && l.AddedDate.Year == DateTime.Now.Year
                         
                         select l);
        }
    }
}