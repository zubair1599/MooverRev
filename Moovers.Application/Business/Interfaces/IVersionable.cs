// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="IVersionable.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Interfaces
{
    using System;

    public interface IVersionable
    {
        string CreatedBy { get; set; }

        DateTime? CreatedOn { get; set; }

        string ModifiedBy { get; set; }

        DateTime? ModifiedOn { get; set; }
    }
}