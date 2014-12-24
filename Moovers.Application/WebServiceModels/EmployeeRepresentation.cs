// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EmployeeRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System;
    using System.Linq;

    using Business.Models;

    public class EmployeeRepresentation
    {
        public EmployeeRepresentation(aspnet_User auth)
        {
            user_name = auth.UserName;
            employee_id = auth.Employee_aspnet_User_Rel.FirstOrDefault().EmployeeID;
            first_name = auth.Employee_aspnet_User_Rel.FirstOrDefault().Employee.NameFirst;
            last_name = auth.Employee_aspnet_User_Rel.FirstOrDefault().Employee.NameLast;
            display_name = auth.Employee_aspnet_User_Rel.FirstOrDefault().Employee.DisplayName();
            short_name = auth.Employee_aspnet_User_Rel.FirstOrDefault().Employee.DisplayShortName();
        }

        public string user_name { get; set; }

        public Guid employee_id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string display_name { get; set; }

        public string short_name { get; set; }

        public bool is_driver { get; set; }
    }
}