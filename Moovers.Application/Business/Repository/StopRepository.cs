// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="StopRepository.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Models;

    public class StopRepository : RepositoryBase<Stop>
    {
        public override Stop Get(Guid id)
        {
            return db.Stops.SingleOrDefault(i => i.StopID == id);
        }

        public Stop GetWithAddress(Guid id)
        {
            return db.Stops.Include("Address").SingleOrDefault(i => i.StopID == id);
        }

        public void Remove(Stop stop)
        {
            List<Room> rooms = stop.Rooms.ToList();
            foreach (Room i in rooms)
            {
                List<Room_InventoryItem> list = i.Room_InventoryItems.ToList();
                foreach (Room_InventoryItem rel in list)
                {
                    db.Room_InventoryItem.DeleteObject(rel);
                }

                db.Rooms.DeleteObject(i);
            }

            db.Stops.DeleteObject(stop);
        }
    }
}