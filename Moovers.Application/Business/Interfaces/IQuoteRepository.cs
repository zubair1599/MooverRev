// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="IQuoteRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Interfaces
{
    using System;
    using System.Linq;

    using Business.Models;

    public interface IQuoteRepository
    {
        Quote Get(Guid id);

        IQueryable<Quote> GetAll();

        IQueryable<Quote> GetForFranchise(Guid franchiseID);

        Quote Get(string lookup);

        IQueryable<Quote> GetWithValuation(Guid franchiseID, DateTime start, DateTime end);

        IQueryable<Quote> GetForAccount(Guid accountid);

        void Save(ApplicationType applicationType = ApplicationType.Crm);
    }
}