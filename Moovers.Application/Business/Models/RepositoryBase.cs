// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="RepositoryBase.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Objects;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    using Business.Interfaces;

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "*", Justification = "Odd naming rules in this class make many other functions easier to read.")]
    public abstract class RepositoryBase
    {
        protected RepositoryBase()
        {
            db = new MooversCRMEntities();
        }

        public MooversCRMEntities db { get; internal set; }

        public virtual void Save(ApplicationType applicationType = ApplicationType.Crm)
        {
            if (applicationType == ApplicationType.Crm)
            {
                IEnumerable<ObjectStateEntry> added = db.ObjectStateManager.GetObjectStateEntries(EntityState.Added);
                IEnumerable<ObjectStateEntry> changed = db.ObjectStateManager.GetObjectStateEntries(EntityState.Modified);

                IEnumerable<IVersionable> addedentities = added.Select(en => en.Entity).OfType<IVersionable>();
                IEnumerable<IVersionable> changedentities = changed.Select(en => en.Entity).OfType<IVersionable>();

                if (addedentities.Any())
                {
                    SetVersion(addedentities, ChangeType.Added);
                }

                if (changedentities.Any())
                {
                    SetVersion(changedentities, ChangeType.Modified);
                }
            }

            //var changed = db.ObjectStateManager.GetObjectStateEntries(System.Data.EntityState.Modified | System.Data.EntityState.Added);
            //var quotes = changed.Select(c => c.Entity).OfType<Quote>();
            //if (quotes.Any())
            //{
            //    var repo = new QuoteHistoryRepository();
            //    foreach (var q in quotes.Where(q => q.QuoteID != Guid.Empty))
            //    {
            //        var hist = q.AsQuoteHistory();
            //        if (HttpContext != null)
            //        {
            //            hist.ModifiedBy = HttpContext.User.Identity.Name;
            //        }

            //        repo.Add(q.AsQuoteHistory());
            //        repo.Save();
            //    }
            //}

            db.SaveChanges();
        }

        private void SetVersion(IEnumerable<IVersionable> versionablesList, ChangeType changeType)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;
            DateTime datetime = DateTime.Now;

            foreach (IVersionable versionable in versionablesList)
            {
                if (changeType == ChangeType.Added)
                {
                    versionable.CreatedBy = username;
                    versionable.CreatedOn = datetime;
                }
                else if (changeType == ChangeType.Modified)
                {
                    versionable.ModifiedBy = username;
                    versionable.ModifiedOn = datetime;
                }
            }
        }
    }

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "*", Justification = "Odd naming rules in this class make many other functions easier to read.")]
    public abstract class RepositoryBase<T> : RepositoryBase, IRepository<T>
        where T : class
    {
        public abstract T Get(Guid id);

        public void SetBase(RepositoryBase baseRepo)
        {
            db = baseRepo.db;
        }

        public virtual void Add(T item)
        {
            db.CreateObjectSet<T>().AddObject(item);
        }
    }

    public enum ApplicationType
    {
        Crm = 1,

        Api = 2
    }

    public enum ChangeType
    {
        Added,

        Modified
    }
}