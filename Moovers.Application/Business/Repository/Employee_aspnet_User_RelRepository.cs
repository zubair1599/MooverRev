// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Employee_aspnet_User_RelRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System;
    using System.Linq;

    using Business.Models;

    public class Employee_aspnet_User_RelRepository : RepositoryBase<Employee_aspnet_User_Rel>
    {
        public override Employee_aspnet_User_Rel Get(Guid id)
        {
            return db.Employee_aspnet_User_Rel.SingleOrDefault(i => i.aspnet_UserID == id);
        }

        public override void Save(ApplicationType applicationType = ApplicationType.Crm)
        {
            base.Save(applicationType);
        }

        public void RemoveForUser(Guid userid)
        {
            var items = db.Employee_aspnet_User_Rel.Where(i => i.aspnet_UserID == userid);
            foreach (var item in items)
            {
                db.Employee_aspnet_User_Rel.DeleteObject(item);
            }
        }
    }
}