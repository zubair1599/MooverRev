using System;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.ViewModels;

namespace Business.Repository.Models
{
    public class CaseRepository : RepositoryBase<Case>
    {
        public override Case Get(Guid id)
        {
            return db.Cases.SingleOrDefault(i => i.CaseID == id);
        }

        public IOrderedQueryable<Case> GetCase()
        {
            return (from clm in db.Cases
                where (clm.Status == (int)CaseStatus.Pending || clm.Status == (int)CaseStatus.Inprocess) && clm.CaseSubmitStatus == true
                select clm).OrderByDescending(i => i.Created);
        }

        public IOrderedQueryable<Case> GetClosedClaim()
        {
            return (from clm in db.Cases
                where clm.Status == (int)CaseStatus.Closed && clm.CaseSubmitStatus == true

                select clm).OrderByDescending(i => i.Created);
        }
        public IOrderedQueryable<Claim> GetClaims(string id)
        {
            Guid guid = new Guid(id);
            return (from clm in db.Claims
                where clm.CaseID == guid
                select clm).OrderByDescending(i => i.ClaimID);
        }
        public string GetShipperName(Guid quoteId)
        {
            return (from clm in db.Cases
                where clm.QuoteID == quoteId
                select clm).FirstOrDefault().Quote.Account.PersonAccount.FirstName;
        }
    }
}