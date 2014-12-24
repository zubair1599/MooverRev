// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="RoleAppRelationRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System.Collections.Generic;
    using System.Linq;

    using Business.Models;

    public class RoleAppRelationRepository : RepositoryBase<Role_App_Rel>
    {
        public override Role_App_Rel Get(System.Guid id)
        {
            return db.Role_App_Rel.FirstOrDefault(model => model.Id == id);
        }

        public IEnumerable<Role_App_Rel> GetAll()
        {
            return db.Role_App_Rel.ToList();
        }
    }
}