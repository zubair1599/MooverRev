// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="UserRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System;

    using Business.Models;

    public class UserRepresentation
    {
        public UserRepresentation()
        {
        }

        public UserRepresentation(EmployeeAuthorization empAuth) : base()
        {
            user_id = empAuth.UserID;
        }

        public Guid user_id { get; set; }

        public string password { get; set; }

        public string user_name { get; set; }
        public bool success { get; set; }
    }
}