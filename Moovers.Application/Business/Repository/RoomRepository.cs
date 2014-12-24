// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="RoomRepository.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Linq;

    using Business.JsonObjects;
    using Business.Models;

    public class RoomRepository : RepositoryBase<Room>
    {
        private static readonly Func<MooversCRMEntities, Guid, IQueryable<Room_InventoryItem>> CompiledGetItems =
            CompiledQuery.Compile<MooversCRMEntities, Guid, IQueryable<Room_InventoryItem>>(
                (db, id) =>
                    (db.Room_InventoryItem.Include("InventoryItem")
                        .Include("Room_InventoryItem_Option")
                        .Include("InventoryItem.InventoryItemQuestions")
                        .Include("InventoryItem.InventoryItemQuestions.InventoryItemQuestionOptions")
                        .Where(rel => rel.RoomID == id)));

        private static readonly Func<MooversCRMEntities, Guid, Room> CompiledGetByID =
            CompiledQuery.Compile<MooversCRMEntities, Guid, Room>(
                (db, id) => db.Rooms.Include("Room_Box_Rel").Include("Room_Box_Rel.Box").Include("Room_InventoryItems").SingleOrDefault(i => i.RoomID == id));

        public override Room Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public void Remove(Room item)
        {
            ClearItems(item);
            db.Rooms.DeleteObject(item);
        }

        internal IEnumerable<ItemCollection> GetItems(Guid roomid)
        {
            List<Room_InventoryItem> items = CompiledGetItems(db, roomid).ToList();
            return items.Where(i => i.Count > 0).OrderBy(i => i.Sort).ToList().Select(i => i.ToJsonObject());
        }

        private void ClearItems(Room room)
        {
            foreach (Room_InventoryItem item in room.Room_InventoryItems.ToList())
            {
                db.Room_InventoryItem.DeleteObject(item);
            }
        }
    }
}