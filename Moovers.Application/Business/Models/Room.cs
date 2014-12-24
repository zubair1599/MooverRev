using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Business.Repository.Models;
using Business.Utility;
using Business.ViewModels;

namespace Business.Models
{
    using LinqKit;

    public partial class Room 
    {
        public Room()
        { 
        }

        public Room(JsonObjects.RoomJson json)
        {
            this.Name = json.Type;
            this.Description = json.Description;
            this.StopID = json.StopID;
            this.SetItems(json.Items);
            this.SetBoxCount(json.Boxes);
            this.Pack = json.Pack;
            this.Sort = json.Sort;
            this.IsUnassigned = json.IsUnassigned;
        }

        public IEnumerable<JsonObjects.ItemCollection> GetItems()
        {
            var repo = new RoomRepository();
            return repo.GetItems(this.RoomID);
        }

        private void SetBoxCount(IEnumerable<JsonObjects.BoxRelJson> boxes)
        {
            this.Room_Box_Rel.DeleteAll();
            var ret = new List<Room_Box_Rel>();
            foreach (var box in boxes)
            {
                if (box.BoxTypeBoxID == default(Guid))
                {
                    continue;
                }

                var rel = new Room_Box_Rel {
                    BoxTypeID = box.BoxTypeBoxID,
                    Count = box.Count
                };

                ret.Add(rel);
                this.Room_Box_Rel.Add(rel);
            }
        }

        /// <param name="items"></param>
        /// <returns>List of changed inventory items</returns>
        public IEnumerable<Room_InventoryItem> SetItems(IEnumerable<JsonObjects.ItemCollection> items)
        {
            var current = this.Room_InventoryItems.ToList();
            this.Room_InventoryItems.DeleteAll();

            var ret = new List<Room_InventoryItem>();
            foreach (var item in items)
            {
                var rel = new Room_InventoryItem {
                    InventoryItemID = item.Item.ItemID,
                    Count = item.Count,
                    Sort = item.Sort,
                    StorageCount = item.StorageCount
                };

                if (item.AdditionalInfo != null)
                {
                    foreach (var i in item.AdditionalInfo)
                    {
                        rel.AddAdditionalInfo(i);
                    }
                }

                if (string.IsNullOrEmpty(item.Notes))
                {
                    rel.RoomInventoryItemNotes.Add(new RoomInventoryItemNote() { DateCreated = DateTime.Now, NoteText = item.Notes });
                }

                var existing = current.FirstOrDefault(i => i.InventoryItemID == item.Item.ItemID);
                if (existing == null || existing.Count != item.Count)
                {
                    ret.Add(rel);
                }

                this.Room_InventoryItems.Add(rel);
            }

            return ret;
        }

        public JsonObjects.RoomJson ToJsonObject()
        {
            return new JsonObjects.RoomJson()
            {
                Description = this.Description,
                StopName = this.Stop.Address.DisplaySmall(),
                StopID = this.Stop.StopID,
                RoomID = this.RoomID.ToString(),
                Type = this.Name,
                Items = this.GetItems(),
                Sort = this.Sort,
                Pack = this.Pack,
                Boxes = this.Room_Box_Rel.Select(i => new JsonObjects.BoxRelJson() {
                    BoxTypeBoxID = i.BoxTypeID,
                    Count = i.Count,
                    Name = i.Box.Name
                }),
                IsUnassigned = this.IsUnassigned
            };
        }
    }
}
