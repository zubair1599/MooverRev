// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="InventoryItem.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Interfaces;
    using Business.JsonObjects;

    public partial class InventoryItem : IVersionable
    {
        public IEnumerable<string> Aliases
        {
            private get
            {
                if (String.IsNullOrEmpty(AliasString))
                {
                    return Enumerable.Empty<string>();
                }

                return AliasString.Split(',').Where(i => !String.IsNullOrWhiteSpace(i)).Select(i => i.Trim());
            }

            set { AliasString = String.Join(",", value); }
        }

        /// <summary>
        ///     Whether the item warrants a special alert
        /// </summary>
        /// <returns></returns>
        public bool IsSpecialItem()
        {
            if (LiabilityCost > 15)
            {
                return true;
            }

            if (CubicFeet > 100)
            {
                return true;
            }

            return IsSpecial;
        }

        public IEnumerable<string> GetAlerts()
        {
            var ret = new List<string>();
            return ret;
        }

        public InventoryItem Duplicate()
        {
            var item = new InventoryItem
            {
                KeyCode = KeyCode,
                Name = Name ?? String.Empty,
                PluralName = PluralName ?? String.Empty,
                Aliases = Aliases,
                CubicFeet = CubicFeet,
                Weight = Weight,
                IsBox = IsBox
            };
            return item;
        }

        public ItemJson ToJsonObject()
        {
            return new ItemJson()
            {
                ItemID = ItemID,
                Name = Name,
                Weight = Weight,
                CubicFeet = CubicFeet,
                IsBox = IsBox,
                IsSpecialItem = IsSpecialItem(),
                KeyCode = KeyCode,
                Aliases = Aliases,
                MoversRequired = MoversRequired,
                AdditionalQuestions = InventoryItemQuestions.OrderBy(i => i.Sort).Select(i => i.ToJsonObject()),
                CustomTime = CustomTime,
                Boxes = InventoryItem_Box_Rel.Select(i => new BoxRelJson { BoxTypeBoxID = i.Box.BoxTypeID, Name = i.Box.Name, Count = i.Count })
            };
        }
    }
}