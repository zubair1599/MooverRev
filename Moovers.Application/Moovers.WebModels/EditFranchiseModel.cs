// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="EditEmployeeModel.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.WebModels
{
    using System;
    using System.Collections.Generic;

    using Business.Enums;
    using Business.Models;
    using Business.Repository.Models;

    public class EditFranchiseModel
    {
        public EditFranchiseModel()
        {
        }

        public EditFranchiseModel(Franchise franchise)
        {
            franchise = franchise ?? new Franchise();

            this.Franchise = franchise;

           
        }

        public EditFranchiseModel(Franchise franchise, AddressModel address, bool isEdit)
            : this(franchise)
        {
            this.Address = address;
            IsEdit = isEdit;
        }

        public bool IsEdit { get; set; }

        public Franchise Franchise { get; set; }
        public AddressModel Address { get; set; }

        


      
    }
}