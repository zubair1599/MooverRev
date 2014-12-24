// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="aspnet_Users_MembershipRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System;
    using System.Linq;

    using Business.Models;

    public class AspnetUsersMembershipRepository : RepositoryBase<aspnet_Membership>
    {
        public override aspnet_Membership Get(Guid id)
        {
            return db.aspnet_Membership.SingleOrDefault(i => i.UserId == id);
        }

        public override void Save(ApplicationType applicationType = ApplicationType.Crm)
        {
            base.Save(applicationType);
        }
    }
}