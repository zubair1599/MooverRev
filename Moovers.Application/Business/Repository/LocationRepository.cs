// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="LocationRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System;
    using System.Linq;

    using Business.Models;

    public class LocationRepository : RepositoryBase<Location>
    {
        public override Location Get(Guid id)
        {
            return db.Locations.FirstOrDefault(x => x.LocationId == id);
        }

        public IOrderedQueryable<Location> GetAll()
        {
            return db.Locations.OrderBy(location => location.StoreId);
        }
    }
}