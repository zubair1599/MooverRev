// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Location.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using Business.Enums;

    public partial class Location
    {
        public LocationType LocationType
        {
            get { return (LocationType)LocationTypeId; }
            set { LocationTypeId = (int)value; }
        }
    }
}