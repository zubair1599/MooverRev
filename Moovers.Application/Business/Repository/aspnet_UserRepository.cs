// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="aspnet_UserRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Business.Models;

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "*", Justification = "Risky to change manually")]
    public class aspnet_UserRepository : RepositoryBase<aspnet_User>
    {
        private static readonly Func<MooversCRMEntities, Guid, aspnet_User> CompiledGetByID =
            CompiledQuery.Compile<MooversCRMEntities, Guid, aspnet_User>(
                (db, id) => db.aspnet_Users.Include("Employee_aspnet_User_Rel").SingleOrDefault(i => i.UserId == id));

        private static readonly Func<MooversCRMEntities, string, aspnet_User> CompiledGetByUsername =
            CompiledQuery.Compile<MooversCRMEntities, string, aspnet_User>((db, id) => db.aspnet_Users.SingleOrDefault(i => i.UserName == id));

        public override aspnet_User Get(Guid id)
        {
            // aspusers are stored in app-cache, since there are few users and they're fetched often
            var cacheRepo = new CacheRepository();
            string key = "USERID_" + id.ToString();
            if (!cacheRepo.Contains(key))
            {
                aspnet_User user = CompiledGetByID(db, id);
                cacheRepo.Store(key, user);
            }

            return cacheRepo.Get<aspnet_User>(key);
        }

        public aspnet_User GetNonCachedUser(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public aspnet_User Get(string username)
        {
            username = (username ?? String.Empty).Trim();
            if (String.IsNullOrEmpty(username))
            {
                return default(aspnet_User);
            }

            // aspusers are stored in app-cache, since there are few users and they're fetched often
            var cacheRepo = new CacheRepository();
            string key = "USERNAME_" + username;
            if (!cacheRepo.Contains(key))
            {
                aspnet_User user = CompiledGetByUsername(db, username);
                if (user == null)
                {
                    return default(aspnet_User);
                }

                cacheRepo.Store(key, user);
            }

            return cacheRepo.Get<aspnet_User>(key);
        }

        public aspnet_User GetByEmail(string email)
        {
            email = (email ?? String.Empty).Trim().ToLower();
            var cacheRepo = new CacheRepository();
            string key = "USEREMAIL_" + email;
            if (!cacheRepo.Contains(key))
            {
                aspnet_User user = db.aspnet_Users.SingleOrDefault(u => u.aspnet_Membership.Email.ToLower() == email);
                cacheRepo.Store(key, user);
            }

            return cacheRepo.Get<aspnet_User>(key);
        }

        public override void Save(ApplicationType applicationType = ApplicationType.Crm)
        {
            // when we update AspUserRepo, bust all app-cache
            var repo = new CacheRepository();
            repo.Clear("USER");
            base.Save(applicationType);
        }

        public IQueryable<aspnet_User> GetSalesPeople()
        {
            return (from user in db.aspnet_Users where user.aspnet_Users_Profile.FirstOrDefault().ShowInSearch select user);
        }

        public aspnet_User GetRandomSalesPerson()
        {
            aspnet_User[] sales = GetSalesPeople().ToArray();
            int idx = new Random().Next(0, sales.Count());
            return sales[idx];
        }

        public IEnumerable<aspnet_User> GetAll()
        {
            return db.aspnet_Users.OrderBy(u => u.UserName);
        }

        public IEnumerable<aspnet_User> GetForFilter()
        {
            return db.aspnet_Users.Where(i => i.aspnet_Users_Profile.Any() && i.aspnet_Users_Profile.FirstOrDefault().ShowInSearch);
        }
    }
}