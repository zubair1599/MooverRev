// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="IRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Interfaces
{
    using System;

    using Business.Models;

    public interface IRepository<T> : IRepository
    {
        T Get(Guid id);

        void Add(T item);
    }

    public interface IRepository
    {
        void Save(ApplicationType applicationType = ApplicationType.Crm);
    }
}