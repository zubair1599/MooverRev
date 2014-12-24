// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="RoomRepresentation.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Models;

    using WebGrease.Css.Extensions;

    public class RoomRepresentation
    {
        public RoomRepresentation(Room room)
        {
            room_id = room.RoomID.ToString();
            name = room.Name;
            description = room.Description;
            inventories = new List<RoomInventoryItemRepresentation>();
            room.Room_InventoryItems.ForEach(
                item =>
                {
                    var roomitem = new RoomInventoryItemRepresentation(item);
                    inventories.Add(roomitem);
                });
        }

        public RoomRepresentation()
        {
        }

        public string room_id { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public List<RoomInventoryItemRepresentation> inventories { get; set; }
    }

    public class RoomInventoryItemRepresentation
    {
        public RoomInventoryItemRepresentation(Room_InventoryItem item)
        {
            if (item != null)
            {
                addendums = new List<RoomItemAddendum>();
                notes = new List<RoomInventoryItemNoteRepresentation>();
                item.Room_InventoryItem_Option.ToList().ForEach(
                    o =>
                    {
                        if (o.Option.HasValue)
                        {
                            addendums.Add(new RoomItemAddendum(o));
                        }
                        else
                        {
                            addendums.Add(new RoomItemAddendum(o));
                        }
                    });

                item.RoomInventoryItemNotes.ToList().ForEach(
                    note =>
                    {
                        if (note != null)
                        {
                            notes.Add(new RoomInventoryItemNoteRepresentation(note));
                        }
                    });

                item_id = item.InventoryItemID.ToString();
                name = item.InventoryItem.Name;
                count = item.Count;
                roomid = item.RoomID.ToString();
                relationshipid = item.RelationshipID.ToString();
                storagecount = item.StorageCount;
                liability_cost = item.InventoryItem.LiabilityCost;
            }
        }

        public RoomInventoryItemRepresentation()
        {
        }

        public string item_id { get; set; }

        public string name { get; set; }

        public int count { get; set; }

        public int storagecount { get; set; }

        public string relationshipid { get; set; }

        public InventoryItemRepresentation item { get; set; }

        public string roomid { get; set; }

        public decimal? liability_cost { get; set; }

        public List<RoomInventoryItemNoteRepresentation> notes { get; set; }

        public List<RoomItemAddendum> addendums { get; set; }
    }

    public class RoomInventoryItemNoteRepresentation
    {
        public RoomInventoryItemNoteRepresentation()
        {
        }

        public RoomInventoryItemNoteRepresentation(RoomInventoryItemNote note)
        {
            note_id = note.NoteId.ToString();
            stampdatetime = note.DateCreated;
            note_text = note.NoteText;
            room_inventory_item_id = note.RoomInventoryItemId.ToString();
        }

        public string note_id { get; set; }

        public DateTime? stampdatetime { get; set; }

        public string note_text { get; set; }

        public string room_inventory_item_id { get; set; }
    }

    public class RoomItemAddendum
    {
        public RoomItemAddendum(Room_InventoryItem_Option roomInventoryItemQuestion)
        {
            room_addendum_id = roomInventoryItemQuestion.AdditionalInfoID.ToString();
            addendum_id = roomInventoryItemQuestion.QuestionID.ToString();
            sub_addendum_id = roomInventoryItemQuestion.Option.ToString();
            room_item_id = roomInventoryItemQuestion.RelationshipID.ToString();
            addendum_name = roomInventoryItemQuestion.Option.HasValue
                ? roomInventoryItemQuestion.InventoryItemQuestionOption.Option
                : roomInventoryItemQuestion.InventoryItemQuestion.QuestionText;
        }

        public RoomItemAddendum()
        {
        }

        public string room_addendum_id { get; set; }

        public string addendum_id { get; set; }

        public string sub_addendum_id { get; set; }

        public string room_item_id { get; set; }

        public string addendum_name { get; set; }
    }

    public class Addendum
    {
        public Addendum(InventoryItemQuestion inventoryItemQuestion)
        {
            addendum_id = inventoryItemQuestion.QuestionID.ToString();
            addendum_description = inventoryItemQuestion.QuestionText;
            weight = inventoryItemQuestion.Weight;
            cubic_feet = inventoryItemQuestion.CubicFeet;
            time = inventoryItemQuestion.Time;
        }

        public Addendum(InventoryItemQuestionOption question)
        {
            addendum_id = question.QuestionID.ToString();
            sub_addendum_id = question.OptionID.ToString();
            addendum_description = question.Option;
            weight = question.Weight;
            cubic_feet = question.CubicFeet;
            time = question.Time;
            sort = question.Sort;
        }

        public Addendum()
        {
        }

        public string addendum_id { get; set; }

        public string sub_addendum_id { get; set; }

        public string addendum_description { get; set; }

        public decimal? weight { get; set; }

        public decimal? cubic_feet { get; set; }

        public decimal? time { get; set; }

        public int sort { get; set; }
    }
}