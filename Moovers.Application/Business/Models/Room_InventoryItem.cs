// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Room_InventoryItem.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System.Linq;

    using Business.Interfaces;
    using Business.JsonObjects;

    public partial class Room_InventoryItem : IVersionable
    {
        public void AddAdditionalInfo(AdditionalInfo info)
        {
            var rel = new Room_InventoryItem_Option { Option = info.OptionID, QuestionID = info.QuestionID };
            Room_InventoryItem_Option.Add(rel);
        }

        public ItemCollection ToJsonObject()
        {
            return new ItemCollection()
            {
                Item = InventoryItem.ToJsonObject(),
                RoomInverntoryItemID = RelationshipID,
                Count = Count,
                Sort = Sort,
                StorageCount = StorageCount,
                IsBox = InventoryItem.IsBox,
                Notes = RoomInventoryItemNotes.Count > 0 ? RoomInventoryItemNotes.First().NoteText : "",
                IsSpecialItem = InventoryItem.IsSpecialItem(),
                AdditionalInfo =
                    Room_InventoryItem_Option.Select(
                        i =>
                            new AdditionalInfo()
                            {
                                OptionID = i.Option,
                                QuestionID = i.QuestionID,
                                DisplayName = i.Option.HasValue ? i.InventoryItemQuestionOption.Option : i.InventoryItemQuestion.ShortName
                            })
            };
        }
    }
}