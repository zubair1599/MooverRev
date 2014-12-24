// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="aspnet_Users_ProfileRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Linq;

    using Business.Models;

    public class aspnet_Users_ProfileRepository : RepositoryBase<aspnet_Users_Profile>
    {
        public override aspnet_Users_Profile Get(Guid id)
        {
            return db.aspnet_Users_Profile.SingleOrDefault(i => i.UserID == id);
        }

        public override void Save(ApplicationType applicationType = ApplicationType.Crm)
        {
            base.Save(applicationType);
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.aspnet_Users_Profile.Where(i => i.UserID == userid);
            foreach (var item in items)
            {
                db.aspnet_Users_Profile.DeleteObject(item);
            }
        }
    }
}