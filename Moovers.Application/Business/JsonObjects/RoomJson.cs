using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public struct RoomJson
    {
        public string RoomID { get; set; }

        public string Type { get; set; }

        public string Description { get; set; }

        public Guid StopID { get; set; }

        public string StopName { get; set; }

        public IEnumerable<ItemCollection> Items { get; set; }

        public int Sort { get; set; }

        public IEnumerable<BoxRelJson> Boxes { get; set; }

        public bool Pack { get; set; }

        public bool IsUnassigned { get; set; }

        public static RoomJson GetFromMooversStorage()
        {
            return new RoomJson() {
                Description = "Moovers Storage",
                Type = "Moovers Storage",
                Pack = false,
                RoomID = String.Empty,
                Sort = 0,
                Boxes = Enumerable.Empty<Business.JsonObjects.BoxRelJson>()
            };
        }
    }
}
