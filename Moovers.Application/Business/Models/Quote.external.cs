using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Business.Repository.Models;

namespace Business.Models
{
    public partial class Quote
    {
        public string GetWebComments()
        {
            if (!String.IsNullOrWhiteSpace(this.CustomInventoryData))
            {
                var items = Regex.Split(this.CustomInventoryData, @"\|\|\|");
                if (items.Length > 1 && !String.IsNullOrWhiteSpace(items[0]))
                {
                    return items[0];
                }
            }

            return null;
        }

        public IEnumerable<string> GetCustomInventory()
        {
            if (!String.IsNullOrWhiteSpace(this.CustomInventoryData))
            {
                var items = Regex.Split(this.CustomInventoryData, @"\|\|\|");
                if (items.Length >= 5)
                {
                    var cust = items[4];
                    var tokens = Regex.Split(cust, ";;").Select(i => Regex.Split(i, "::"));
                    if (tokens.All(i => i.Length >= 3))
                    {
                        return tokens.Select(i => i[2] + " - " + i[1].Trim());
                    }
                }
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<JsonObjects.ItemCollection> GetInventoryFromExternal(string inventoryData)
        {
            var items = new List<JsonObjects.ItemCollection>();
            int sort = 0;
            foreach (var pair in Regex.Split(inventoryData, ";;").Select(i => Regex.Split(i, "::")))
            {
                int code;
                int number;
                bool hasCode = int.TryParse(pair[0], out code);
                bool hasNumber = int.TryParse(pair[1], out number);

                if (hasCode && hasNumber)
                {
                    var itemRepo = new InventoryItemRepository();
                    var item = itemRepo.GetByMovepointKeycode(code);
                    if (item != null)
                    {
                        items.Add(new JsonObjects.ItemCollection() {
                            AdditionalInfo = Enumerable.Empty<JsonObjects.AdditionalInfo>(),
                            Item = new JsonObjects.ItemJson() { ItemID = item.ItemID, Name = item.Name },
                            Count = number,
                            IsBox = false,
                            Sort = sort++,
                            StorageCount = 0
                        });
                    }
                }
            }

            return items;
        }

        public void AddInventoryFromExternal(string inventoryData)
        {
            if (!this.Stops.Any())
            {
                return;
            }

            var stop = this.Stops.First();
            var roomJson = new JsonObjects.RoomJson() {
                Description = String.Empty,
                Pack = false,
                RoomID = Utility.General.RandomString(5),
                Sort = 0,
                StopID = stop.StopID,
                Type = "Unassigned",
                IsUnassigned = true,
                Boxes = Enumerable.Empty<JsonObjects.BoxRelJson>()
            };

            try
            {
                var items = this.GetInventoryFromExternal(inventoryData);

                roomJson.Items = items;
                var room = new Room(roomJson);
                stop.Rooms.Add(room);
            }
            catch (Exception e)
            {
                var repo = new ErrorRepository();
                repo.Log(e, inventoryData);
            }
        }
    }
}
