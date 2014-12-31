using System;
using System.Data.Objects;
using System.Linq;
using System.Text.RegularExpressions;
using Business.Models;
using System.Collections.Generic;

namespace Business.Repository
{
    public class AccountRepository : RepositoryBase<Account>
    {
        private static readonly Func<MooversCRMEntities, Guid, Account> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Account>(
            (db, id) => db.Accounts.Include("Account_PhoneNumber_Rel")
                .Include("Account_PhoneNumber_Rel.PhoneNumber")
                .Include("Account_Address_Rel")
                .Include("Account_Address_Rel.Address").SingleOrDefault(i => i.AccountID == id)
            );

        private static readonly Func<MooversCRMEntities, string, Account> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, Account>(
            (db, id) => db.Accounts.SingleOrDefault(i => i.Lookup == id)
            );

        private static readonly Func<MooversCRMEntities, Guid, Account_Customer_Credentials> CompiledGetAccountByUserId = CompiledQuery.Compile<MooversCRMEntities, Guid, Account_Customer_Credentials>(
            (db, userid) => db.Account_Customer_Credentials.SingleOrDefault(o => o.UserId == userid));

        public AccountRepository()
        {
        }

        public AccountRepository(MooversCRMEntities entities)
        {
            this.db = entities;
        }


        private IQueryable<Account> GetAll()
        {
            return db.Accounts.Where(i => !i.IsArchived);
        }

        private IQueryable<Account> GetAll(Guid franchiseID)
        {
            return (from a in this.GetAll()
                where
                    (a.FranchiseID == franchiseID
                     || (a.BillingQuotes.Any(q => q.FranchiseID == franchiseID) || a.ShippingQuotes.Any(q => q.FranchiseID == franchiseID)))
                select a);
        }

        public override Account Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public Account Get(string lookup)
        {
            return CompiledGetByLookup(db, lookup);
        }
        public List<Account> GetAccountFromQuote(string lookup)
        {
            var quote = db.Quotes.Where(q => q.Lookup == lookup).FirstOrDefault();
            return new List<Account> { quote.Account, quote.ShippingAccount };
        }
        public void RemoveEmail(Account account, EmailAddressType type)
        {
            var rel = account.Account_EmailAddress_Rel.FirstOrDefault(i => i.Type == (int)type);
            if (rel != null)
            {
                db.Account_EmailAddress_Rel.DeleteObject(rel);
            }
        }

        public void RemovePhone(Account account, PhoneNumberType type)
        {
            var rel = account.Account_PhoneNumber_Rel.FirstOrDefault(i => i.Type == (int)type);
            if (rel != null)
            {
                db.Account_PhoneNumber_Rel.DeleteObject(rel);
            }
        }

        private IQueryable<Account> SearchNames(Guid? franchiseID, string search)
        {
            search = (search ?? String.Empty).Trim();
            var collection = (franchiseID.HasValue) ? this.GetAll(franchiseID.Value) : this.GetAll();
            var accounts = (from a in collection
                from business in db.Accounts.OfType<BusinessAccount>().Where(i => i.AccountID == a.AccountID).DefaultIfEmpty()
                from person in db.Accounts.OfType<PersonAccount>().Where(i => i.AccountID == a.AccountID).DefaultIfEmpty()
                let name = (business != null) ? business.Name : (person.FirstName + " " + person.LastName)
                let sortname = (business != null) ? business.Name : (person.LastName + " " + person.FirstName)
                select new JsonObjects.AccountSearchResult
                {
                    account = a,
                    person = person,
                    business = business,
                    name = name,
                    sortname = sortname
                });

            if (String.IsNullOrEmpty(search))
            {
                return (from a in accounts
                    orderby a.sortname
                    select a.account);
            }

            if (search.Contains("@"))
            {
                return (from a in accounts
                    where a.account.Account_EmailAddress_Rel.Any(e => e.EmailAddress.Email.StartsWith(search))
                    orderby a.name
                    select a.account);
            }

            // match account lookups exactly
            if (Regex.IsMatch(search, @"^A\d+", RegexOptions.IgnoreCase))
            {
                return (from a in accounts
                    where a.account.Lookup == search
                    orderby a.name
                    select a.account);
            }

            if (Regex.IsMatch(search, @"^\d+"))
            {
                return (from a in accounts
                    where a.account.Account_PhoneNumber_Rel.Any(p => p.PhoneNumber.Number.StartsWith(search))
                    orderby a.name
                    select a.account);
            }

            return from a in accounts
                where
                    //score > .7 ||
                    (a.account is PersonAccount && (a.person.FirstName.StartsWith(search) || a.person.LastName.StartsWith(search)))
                    || (a.account is PersonAccount && (a.person.FirstName + " " + a.person.LastName).StartsWith(search))
                    || (a.account is BusinessAccount && a.business.Name.StartsWith(search))
                orderby (a.account is PersonAccount ? (a.person.LastName + " " + a.person.FirstName) : a.business.Name)
                select a.account;
        }

        public Utility.PagedResult<JsonObjects.SmallAccountJson> Search(Guid? franchiseID, string search, int page, int pageSize)
        {
            search = (search ?? String.Empty).Trim();
            var results = SearchNames(franchiseID, search);
            var items = results.Skip(page * pageSize).Take(pageSize).ToList().Select(a => new JsonObjects.SmallAccountJson()
            {
                Name = a.DisplayName,
                AccountID = a.AccountID,
                Lookup = a.Lookup,
                Type = a is BusinessAccount ? "Business" : "Person",
                City = a.GetAddress(AddressType.Mailing) != null ? a.GetAddress(AddressType.Mailing).City : String.Empty,
                State = a.GetAddress(AddressType.Mailing) != null ? a.GetAddress(AddressType.Mailing).State : String.Empty
            });

            var ret = new Utility.PagedResult<JsonObjects.SmallAccountJson>();
            ret.PageSize = pageSize;
            ret.CurrentPage = page;
            ret.Items = items;
            ret.TotalCount = results.Count();
            return ret;
        }

        public Account GetAccountByUserId(Guid userId)
        {

            return CompiledGetAccountByUserId(db, userId).Account;

        }
    }
}