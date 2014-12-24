// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="StopRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Reflection;
using WebGrease.Css.Extensions;

namespace WebServiceModels
{
    using System.Collections.Generic;
    using System.Linq;

    using Business.Enums;
    using Business.Models;
    using Business.ToClean;

    public class StopRepresentation
    {
        public StopRepresentation()
        {
        }

        public StopRepresentation(Stop stop)
        {
            StopJson stopJson = stop.ToJsonObject(false);
            stop_id = stopJson.id;
            address_id = stopJson.addressid;
            address1 = stopJson.street1;
            address2 = stopJson.street2;
            city = stopJson.city;
            state = stopJson.state;
            postal_code = stopJson.zip;
            priority = stopJson.sort;
            storage_days = stopJson.storageDays;
            rooms = new List<RoomRepresentation>();
            stop.Rooms.ToList().ForEach(r => rooms.Add(new RoomRepresentation(r)));

            condition = new Conditions()
            {
                address_type = stopJson.addressType,
                floors = stopJson.floor,
                elevator = stopJson.elevatorType,
                walking = stopJson.walkDistance,
                parking_type = stopJson.parkingType,
                stairs_inside = stopJson.insideStairsCount,
                stairs_outside = stopJson.outsideStairsCount,
                inside_stairs_type = stopJson.insideStairsType,
                outside_stairs_type = stopJson.outsideStairsType,
                apartment_complex = stopJson.apartmentComplex,
                complex_gate_code = stopJson.apartmentGateCode ?? "",
                dock_high = stopJson.dock,
                requires_liftgate = stopJson.liftgate
            };
            statuses = new List<QuoteStopStatusRepresentation>();

            stop.QuoteStatus.OrderByDescending(qs=>qs.StatusUpdateTime).ForEach(qs => statuses.Add(new QuoteStopStatusRepresentation(qs)));
            
        }

        public string stop_id { get; set; }

        public string address_id { get; set; }

        public string state { get; set; }

        public string address1 { get; set; }

        public string address2 { get; set; }

        public string postal_code { get; set; }

        public string city { get; set; }

        public int priority { get; set; }

        public bool verified { get; set; }

        public string verified_address { get; set; }

        public int? storage_days { get; set; }

        public string contact { get; set; }

        public Conditions condition { get; set; }

        public List<RoomRepresentation> rooms { get; set; }

        public string stop_type { get; set; }

        public string quote_id { get; set; }

        public List<QuoteStopStatusRepresentation> statuses { get; set; }
    }

    public class Conditions
    {
        public ParkingType parking_type { get; set; }

        public ElevatorType elevator { get; set; }

        public StopAddressType address_type { get; set; }

        public StairType outside_stairs_type { get; set; }

        public StairType inside_stairs_type { get; set; }

        public int floors { get; set; }

        public int stairs_inside { get; set; }

        public int stairs_outside { get; set; }

        public int walking { get; set; }

        public bool requires_liftgate { get; set; }

        public bool dock_high { get; set; }

        public string apartment_complex { get; set; }

        public string complex_gate_code { get; set; }
    }

    public class QuoteStopStatusRepresentation
    {
        public Guid quote_status_id { get; set; }
        public QuoteFieldStatus status_type { get; set; }

        public string name { get; set; }
        public Guid stop_id { get; set; }
        public string stop_name { get; set; }
        public DateTime updated_time { get; set; }
        
        public QuoteStopStatusRepresentation() { }

        public QuoteStopStatusRepresentation(QuoteStatu status)
        {
            this.quote_status_id = status.QuoteStatusId;
            this.stop_id = status.StopId;
            this.stop_name = status.Stop.Sort.ToString();
            this.status_type = (QuoteFieldStatus)status.Status_Type;
            this.name = GetEnumDescription((QuoteFieldStatus) status.Status_Type);
            
            this.updated_time = status.StatusUpdateTime;
        }
      
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }
}