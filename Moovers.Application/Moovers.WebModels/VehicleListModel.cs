using System;
using System.Collections.Generic;
using Business.Models;
using Business.Repository.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public class VehicleListModel : SortableModel<VehicleSort?>
    {
        public IEnumerable<Franchise> Franchises { get; set; }

        public IEnumerable<Vehicle> Vehicles { get; set; }

        public override VehicleSort? Sort { get; set; }

        public override bool Desc { get; set; }

        public override IEnumerable<KeyValuePair<string, VehicleSort?>> GetHeaders()
        {
            var ret = new Dictionary<string, VehicleSort?> {
                { "VehicleID", VehicleSort.Lookup }, 
                { "Name", VehicleSort.Name }, 
                { "Franchise", VehicleSort.Franchise }, 
                { "Cubic Feet", VehicleSort.CubicFeet }, 
                { "Length", VehicleSort.Length }, 
                { "Make", VehicleSort.Make }, 
                { "Model", VehicleSort.Model }, 
                { "Year", VehicleSort.Year }, 
                { "Type", default(VehicleSort?) }, 
                { "Description", default(VehicleSort?) }, 
                { String.Empty, default(VehicleSort?) }
            };
            return ret;
        }
    }
}
