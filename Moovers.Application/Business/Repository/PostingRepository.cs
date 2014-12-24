using System;
using System.Data.Objects;
using System.Linq;
using System.Text.RegularExpressions;
using Business.Enums;
using Business.Models;
using Business.ToClean.QuoteHelpers;
using Business.Utility;

namespace Business.Repository.Models
{
    public class PostingRepository : RepositoryBase<Posting>
    {
        private static Func<MooversCRMEntities, Guid, Posting> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Posting>(
            (db, id) => db.Postings.SingleOrDefault(i => i.PostingID == id)
            );

        private static Func<MooversCRMEntities, string, Posting> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, Posting>(
            (db, id) => db.Postings.SingleOrDefault(i => i.Lookup == id)
            );

        private static Func<MooversCRMEntities, Guid, bool, PostSortColumn, IQueryable<Posting>> CompiledGetByCompleted = 
            CompiledQuery.Compile<MooversCRMEntities, Guid, bool, PostSortColumn, IQueryable<Posting>>(
                (db, franchiseid, completed, sort) => 
                    (from post in db.Postings.Include("Posting_Employee_Rel").Include("Posting_Employee_Rel.Employee")
                        .Include("Posting_Vehicle_Rel").Include("Posting_Vehicle_Rel.Vehicle")
                        .Include("Schedule").Include("Schedule.Quote").Include("Schedule.Quote.QuotePayments").Include("Schedule.Quote.QuoteServices")
                        where (post.IsComplete == completed)
                              && post.Quote.FranchiseID == franchiseid
                        ////&& (!start.HasValue ||  post.Schedule.Date >= start)
                        ////&& (!end.HasValue || post.Schedule.Date <= end)
                        select post)
                );

        public override Posting Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public Posting Get(string lookup)
        {
            return CompiledGetByLookup(db, lookup);
        }

        private IQueryable<Posting> OrderPostings(IQueryable<Posting> postings, PostSortColumn sort, bool desc = false)
        {
            if (sort == PostSortColumn.AccountName)
            {
                postings = postings.Select(post => new
                {
                    post = post,
                    personaccount = db.Accounts.OfType<PersonAccount>().FirstOrDefault(a => a.AccountID == post.Schedule.Quote.AccountID),
                    businessaccount = db.Accounts.OfType<BusinessAccount>().FirstOrDefault(a => a.AccountID == post.Schedule.Quote.AccountID)
                }).OrderWithDirection(q => q.personaccount != null ? q.personaccount.LastName + " " + q.personaccount.FirstName : q.businessaccount.Name, desc).Select(q => q.post);
            }

            if (sort == PostSortColumn.Price)
            {
                postings = postings.OrderWithDirection(i =>
                    i.Schedule.Quote.PricingTypeID == (int)QuotePricingType.Hourly
                        ? i.Schedule.Quote.HourlyPrice
                        : i.Schedule.Quote.GuaranteedPrice, desc);
            }

            if (sort == PostSortColumn.QuoteID)
            {
                postings = postings.OrderWithDirection(i => i.Schedule.Quote.Lookup, desc);
            }

            if (sort == PostSortColumn.ServiceDate)
            {
                postings = postings.OrderWithDirection(q => q.Schedule.Date, desc);
            }

            if (sort == PostSortColumn.Employees)
            {
                postings = postings.OrderWithDirection(q => 
                    q.Posting_Employee_Rel.Any() ? q.Posting_Employee_Rel.OrderBy(i => i.Employee.Lookup).FirstOrDefault().Employee.Lookup : String.Empty, desc
                    );
            }

            if (sort == PostSortColumn.Vehicles)
            {
                postings = postings.OrderWithDirection(q =>
                    q.Posting_Vehicle_Rel.Any() ? q.Posting_Vehicle_Rel.OrderBy(i => i.Vehicle.Lookup).FirstOrDefault().Vehicle.Lookup : String.Empty, desc
                    );
            }

            if (sort == PostSortColumn.PostingDate)
            {
                postings = postings.OrderWithDirection(q => q.IsComplete && q.DateCompleted.HasValue ? q.DateCompleted.Value : Date.SmallDatetimeMin, desc);
            }

            if (sort == PostSortColumn.Balance)
            {
                postings = postings.Select(p => new {
                    payments = p.Quote.QuotePayments.Where(i => !i.IsCancelled && i.Success),
                    postedprice = (!p.IsComplete || !p.Quote.FinalPostedPrice.HasValue) ? 0 : p.Quote.FinalPostedPrice.Value,
                    tip = !p.Posting_Employee_Rel.Any() ? 0 : p.Posting_Employee_Rel.Sum(r => r.Tip),
                    post = p
                }).OrderWithDirection(i => !i.payments.Any() ? i.postedprice + i.tip 
                    : (i.postedprice + i.tip) - i.payments.Sum(p => p.Amount), desc).Select(i => i.post);
            }

            return postings;
        }

        private IQueryable<Posting> GetPostingsByCompleted(Guid franchiseID, bool completed, PostSortColumn sort, bool desc = false)
        {
            var postings = CompiledGetByCompleted(db, franchiseID, completed, sort);
            return OrderPostings(postings, sort, desc);
        }

        public IQueryable<Posting> GetPosted(Guid franchiseID, PostSortColumn sort, bool desc = false)
        {
            return GetPostingsByCompleted(franchiseID, true, sort, desc);
        }

        public IQueryable<Posting> GetCancelled(Guid franchiseID, PostSortColumn sort, bool desc)
        {
            var postings = GetPostingsByCompleted(franchiseID, false, sort, desc).Where(i => i.Schedule.IsCancelled && i.Schedule.Quote.StatusID == (int)QuoteStatus.Lost);
            return postings;
        }

        public IQueryable<Posting> GetUnposted(Guid franchiseID, PostSortColumn sort, bool desc = false)
        {
            return GetPostingsByCompleted(franchiseID, false, sort, desc).Where(i => !i.Schedule.IsCancelled);
        }

        public int GetUnpostedCount(Guid franchiseID)
        {
            var date = DateTime.Today;
            return (from i in db.Postings.Where(i => i.Quote.FranchiseID == franchiseID)
                where !i.IsComplete
                      && !i.Schedule.IsCancelled
                      && i.Schedule.Date < date
                select i).Count();
        }

        public IQueryable<Posting> GetSearch(Guid franchiseID, string search, PostSortColumn sort, bool desc = false)
        {
            IQueryable<Posting> ret;

            if (Regex.IsMatch(search, @"^\d+$"))
            {
                ret = (from post in db.Postings
                    where post.Quote.Lookup.Contains(search)
                          || post.Posting_Employee_Rel.Any(r => r.Employee.Lookup == search)
                    select post);
            }
            else {
                ret = (from i in db.Postings
                    select new {
                        person = i.Quote.Account as PersonAccount,
                        business = i.Quote.Account as BusinessAccount,
                        posting = i
                    })
                    .Where(i => 
                        (i.person != null && (i.person.FirstName.Contains(search) || i.person.LastName.Contains(search) || (i.person.FirstName + " " + i.person.LastName).Contains(search)))
                        || (i.business != null && i.business.Name.Contains(search))
                    ).Select(i => i.posting);
            }

            ret = ret.Where(i => i.Quote.FranchiseID == franchiseID);

            return OrderPostings(ret, sort, desc);
        }

        public IQueryable<Posting> GetUnpaid(Guid franchiseID, PostSortColumn sort, bool desc = false)
        {
            var postings = (from post in db.Postings
                let owed = (post.Schedule.Quote.FinalPostedPrice ?? 0)
                let rels = post.Schedule.Quote.Postings.Where(i => i.IsComplete).SelectMany(i => i.Posting_Employee_Rel.Where(m => !m.IsRemoved))
                let tips = rels.Any() ? rels.Sum(i => i.Tip) : 0
                let paid = post.Quote.QuotePayments.Any(q => q.Success && !q.IsCancelled) ? post.Quote.QuotePayments.Where(i => i.Success && !i.IsCancelled).Sum(i => i.Amount) : 0
                where 
                    ((owed + tips - paid) > 0 || (paid - (owed + tips) > 0))
                    && post.IsComplete 
                    && !post.Schedule.IsCancelled
                    && post.Quote.FranchiseID == franchiseID
                    && !post.Quote.IsLostDebt
                select post);
            return OrderPostings(postings, sort, desc);
        }
    }
}