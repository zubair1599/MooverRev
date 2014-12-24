using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public class ItemCollection
    {
        public Guid RoomInverntoryItemID { get; set; }

        public int Count { get; set; }

        public int Sort { get; set; }

        public int StorageCount { get; set; }

        /// <summary>
        /// Whether the item is a Pool Table, Piano, Safe, etc.
        /// </summary>
        public bool IsSpecialItem { get; set; }

        public string Notes { get; set; }

        public ItemJson Item { get; set; }

        public IEnumerable<AdditionalInfo> AdditionalInfo { get; set; }

        public bool IsBox { get; set; }
    }
}
