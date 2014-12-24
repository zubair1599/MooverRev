// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ApplicationRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Models;

    public class ApplicationRepository : RepositoryBase<Application>
    {
        public IEnumerable<Application> GetAll()
        {
            return db.Applications.OrderBy(u => u.AppId);
        }

        public override Application Get(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}