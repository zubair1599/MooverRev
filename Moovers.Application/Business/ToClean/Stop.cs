using System;
using System.Collections.Generic;
using Business.Enums;
using Business.JsonObjects;
using Business.Models;
using Business.ToClean.QuoteHelpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Business.ToClean
{
    public struct StopJson
    {
        public string address { get; set; }
        public string id { get; set; }

        public string addressid { get; set; }

        public string state { get; set; }

        public string street1 { get; set; }

        public string street2 { get; set; }

        public string zip { get; set; }

        public string city { get; set; }

        public int sort { get; set; }

        public bool verified { get; set; }

        public string verifiedAddress { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ParkingType parkingType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ElevatorType elevatorType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StopAddressType addressType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StairType outsideStairsType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public StairType insideStairsType { get; set; }

        public int floor { get; set; }

        public int insideStairsCount { get; set; }

        public int outsideStairsCount { get; set; }

        public int walkDistance { get; set; }

        public bool liftgate { get; set; }

        public bool dock { get; set; }

        public int? storageDays { get; set; }

        public IEnumerable<RoomJson> rooms { get; set; }

        public string apartmentComplex { get; set; }

        public string apartmentGateCode { get; set; }

        public Address GetAddress()
        {
            var address = new Address(this.street1, this.street2, this.city, this.state, this.zip);
            if (!String.IsNullOrEmpty(this.verifiedAddress))
            {
                var candidateAddress = JsonConvert.DeserializeObject<CandidateAddress>(this.verifiedAddress);
                if (candidateAddress != null && !String.IsNullOrEmpty(candidateAddress.delivery_line_1))
                {
                    address.SetVerified(candidateAddress);
                }
            }

            return address;
        }

        public static StopJson GetEmpty()
        {
            return new StopJson()
            {
                city = String.Empty,
                state = String.Empty,
                zip = String.Empty,
                street1 = String.Empty,
                addressType = StopAddressType.House,
                floor = 0,
                elevatorType = ElevatorType.No_Elevator,
                sort = 0,
                walkDistance = 50,
                parkingType = ParkingType.Driveway,
                insideStairsCount = 0,
                outsideStairsCount = 0,
                insideStairsType = StairType.Flight,
                outsideStairsType = StairType.Flight,
                apartmentComplex = String.Empty,
                apartmentGateCode = String.Empty,
                dock = false,
                verified = false,
                liftgate = false
            };
        }
    }
}