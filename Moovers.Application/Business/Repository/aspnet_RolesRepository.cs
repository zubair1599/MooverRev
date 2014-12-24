// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="aspnet_RolesRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Business.Models;

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "*", Justification = "Risky to change manually")]
    public class aspnet_RolesRepository : RepositoryBase<aspnet_Roles>
    {
        public IEnumerable<aspnet_Roles> GetAll()
        {
            return db.aspnet_Roles.OrderBy(u => u.RoleName);
        }

        public override aspnet_Roles Get(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}