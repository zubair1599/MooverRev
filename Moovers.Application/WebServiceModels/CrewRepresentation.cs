// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="CrewRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System;
    using System.Collections.Generic;

    using Business.Models;

    using WebGrease.Css.Extensions;

    public class CrewRepresentation
    {
        public CrewRepresentation(Crew crew)
        {
            crew_id = crew.CrewID.ToString();
            movers = new List<MooverRepresentation>();
            vehicles = new List<VehicleRepresentation>();
            crew.Crew_Employee_Rel.ForEach(ce => movers.Add(new MooverRepresentation(ce)));
            crew.Crew_Vehicle_Rel.ForEach(cv => vehicles.Add(new VehicleRepresentation(cv)));
        }

        public CrewRepresentation()
        {
        }

        public string crew_id { get; set; }
        public List<MooverRepresentation> movers { get; set; }

        public List<VehicleRepresentation> vehicles { get; set; }
    }

    public class MooverRepresentation
    {
        public MooverRepresentation(Crew_Employee_Rel employee)
        {
            if (employee == null)
            {
                return;
            }

            mover_id = employee.Employee.EmployeeID;
            first_name = employee.Employee.NameFirst;
            last_name = employee.Employee.NameLast;
            short_name = employee.Employee.DisplayShortName();
        }

        public Guid mover_id { get; set; }

        public string first_name { get; set; }

        public string last_name { get; set; }

        public string short_name { get; set; }

        public bool is_driver { get; set; }
    }

    public class VehicleRepresentation
    {
        public VehicleRepresentation(Crew_Vehicle_Rel vehicle)
        {
            if (vehicle == null)
            {
                return;
            }

            vehicle_id = vehicle.VehicleID.ToString();
            name = vehicle.Vehicle.Name;
            look_up = vehicle.Vehicle.Lookup;
            cubic_feet = vehicle.Vehicle.CubicFeet;
            length = vehicle.Vehicle.Length;
            make = vehicle.Vehicle.Make;
            model = vehicle.Vehicle.Model;
            year = vehicle.Vehicle.Year;
            type = vehicle.Vehicle.Type;
            description = vehicle.Vehicle.Description;
        }

        public string vehicle_id { get; set; }
        public string look_up { get; set; }
        public string name { get; set; }
        public int? cubic_feet { get; set; }
        public decimal? length { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public int? year { get; set; }
        public string type { get; set; }
        public string description { get; set; }
    }
}