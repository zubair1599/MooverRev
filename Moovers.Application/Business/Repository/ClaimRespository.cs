using System;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.ToClean.QuoteHelpers;

namespace Business.Repository.Models
{
    public class ClaimRespository : RepositoryBase<Claim>
    {
        public override Claim Get(Guid id)
        {
            return db.Claims.SingleOrDefault(i => i.ClaimID == id);
        }

        public IOrderedQueryable<Claim> GetClaims(string id)
        { 
            
            Guid guid = new Guid(id);
            return (from clm in db.Claims
                where clm.CaseID == guid
                select clm).OrderByDescending(i => i.ClaimID);
        }
        public ClaimType GetClaimType(int id)
        {
            return  (ClaimType)id;
          
        }

        public void Remove(Guid id)
        {
            db.Claims.DeleteObject(this.Get(id));
        }
    }
}