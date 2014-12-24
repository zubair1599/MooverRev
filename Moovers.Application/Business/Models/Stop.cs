// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Stop.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Business.Enums;
    using Business.Interfaces;
    using Business.JsonObjects;
    using Business.ToClean;
    using Business.Utility;

    public partial class Stop : IVersionable
    {
        public Stop()
        {
        }

        public Stop(StopJson json) : this()
        {
            CopyJsonProperties(json);
        }

        public StopAddressType AddressType
        {
            get { return (StopAddressType)AddressTypeID; }
            set { AddressTypeID = (int)value; }
        }

        private ParkingType ParkingType
        {
            get { return (ParkingType)ParkingTypeID; }
            set { ParkingTypeID = (int)value; }
        }

        private StairType OutsideStairType
        {
            get { return (StairType)OutsideStairsTypeID; }
            set { OutsideStairsTypeID = (int)value; }
        }

        private StairType InsideStairType
        {
            get { return (StairType)InsideStairsTypeID; }
            set { InsideStairsTypeID = (int)value; }
        }

        private ElevatorType ElevatorType
        {
            get { return (ElevatorType)ElevatorTypeID; }
            set { ElevatorTypeID = (int)value; }
        }

        public static Stop GetStorageStop(Franchise franchise, int sort)
        {
            var json = new StopJson()
            {
                verified = true,
                verifiedAddress = franchise.Address.GetVerified().SerializeToJson(),
                street1 = franchise.Address.Street1,
                street2 = franchise.Address.Street2,
                zip = franchise.Address.Zip,
                state = franchise.Address.State,
                city = franchise.Address.City,
                addressType = StopAddressType.Storage,
                apartmentComplex = "Moovers Storage",
                elevatorType = ElevatorType.No_Elevator,
                walkDistance = 50,
                insideStairsCount = 0,
                insideStairsType = StairType.Flight,
                outsideStairsCount = 0,
                outsideStairsType = StairType.Flight,
                floor = 0,
                liftgate = false,
                dock = true,
                sort = sort
            };

            return new Stop(json);
        }

        public IEnumerable<Room> GetRooms()
        {
            return Rooms.OrderByDescending(i => i.IsUnassigned || i.Name.Contains("Other")).ThenBy(i => i.Sort);
        }

        public IEnumerable<ItemCollection> GetItems()
        {
            return Rooms.SelectMany(i => i.GetItems()).ToList();
        }

        public IEnumerable<string> GetConditions()
        {
            var ret = new List<string>();
            if (InsideStairsCount > 0)
            {
                ret.Add(InsideStairsCount + " Inside " + InsideStairType.GetDescription());
            }

            if (OutsideStairsCount > 0)
            {
                ret.Add(OutsideStairsCount + " Outside " + OutsideStairType.GetDescription());
            }

            if (Floor > 0)
            {
                if (ElevatorType != ElevatorType.No_Elevator)
                {
                    ret.Add(ElevatorType.GetDescription());
                }

                ret.Add("Floor " + Floor.ToString());
            }

            string walkDesc = (WalkDistance <= 1000) ? "< " + WalkDistance.ToString() + " feet" : "> 1000 feet";
            ret.Add("Walk:" + walkDesc);

            return ret;
        }

        public void CopyJsonProperties(StopJson json)
        {
            Address = json.GetAddress();
            AddressType = json.addressType;

            ElevatorType = json.elevatorType;
            InsideStairsCount = json.insideStairsCount;
            InsideStairType = json.insideStairsType;
            OutsideStairsCount = json.outsideStairsCount;
            OutsideStairType = json.outsideStairsType;
            ParkingType = json.parkingType;
            Floor = json.floor;
            WalkDistance = json.walkDistance;

            ApartmentComplex = json.apartmentComplex;
            ApartmentGateCode = json.apartmentGateCode;

            Liftgate = json.liftgate;
            Dock = json.dock;

            Sort = json.sort;
        }

        /// <summary>
        ///     Duplicate all items of a stop, to "address" if applicable
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public Stop Duplicate(Address address = null)
        {
            var ret = new Stop()
            {
                ParkingTypeID = ParkingTypeID,
                ElevatorTypeID = ElevatorTypeID,
                WalkDistance = WalkDistance,
                OutsideStairsCount = OutsideStairsCount,
                OutsideStairsTypeID = OutsideStairsTypeID,
                InsideStairsCount = InsideStairsCount,
                InsideStairsTypeID = InsideStairsTypeID,
                Liftgate = Liftgate,
                Dock = Dock,
                Floor = Floor,
                AdditionalInfo = AdditionalInfo,
                ApartmentComplex = ApartmentComplex,
                ApartmentGateCode = ApartmentGateCode
            };

            foreach (Room room in GetRooms())
            {
                var dupe = new Room(room.ToJsonObject());
                ret.Rooms.Add(dupe);
            }

            ret.Address = (address ?? Address).Duplicate();
            return ret;
        }

        public StopJson ToJsonObject(bool includeItems)
        {
            return new StopJson()
            {
                id = StopID.ToString(),
                address = Address.DisplayString(),
                addressid = Address.AddressID.ToString(),
                street1 = Address.Street1 ?? String.Empty,
                street2 = Address.Street2 ?? String.Empty,
                city = Address.City ?? String.Empty,
                state = Address.State ?? String.Empty,
                zip = Address.Zip ?? String.Empty,
                verified = Address.IsVerified(),
                verifiedAddress = Address.GetVerified().SerializeToJson(),
                addressType = AddressType,
                parkingType = ParkingType,
                outsideStairsType = OutsideStairType,
                outsideStairsCount = OutsideStairsCount,
                insideStairsType = InsideStairType,
                insideStairsCount = InsideStairsCount,
                elevatorType = ElevatorType,
                walkDistance = WalkDistance,
                liftgate = Liftgate,
                dock = Dock,
                floor = Floor,
                apartmentComplex = ApartmentComplex,
                apartmentGateCode = ApartmentGateCode,
                rooms = (includeItems) ? Rooms.Select(i => i.ToJsonObject()) : Enumerable.Empty<RoomJson>(),
                storageDays = StorageDays,
                sort = Sort
            };
        }
    }
}