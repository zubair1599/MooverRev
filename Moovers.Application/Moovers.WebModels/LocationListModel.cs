using System;
using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public sealed class LocationListModel : SortableModel<LocationSort>
    {
        public override LocationSort Sort { get; set; }

        public override bool Desc { get; set; }

        public IEnumerable<Location> Locations { get; set; }

        public override IEnumerable<KeyValuePair<string, LocationSort>> GetHeaders()
        {
            return new Dictionary<string, LocationSort>() {
                { LocationSort.LocationName.GetDescription(), LocationSort.LocationName },
                { LocationSort.LocationType.GetDescription(), LocationSort.LocationType },
                { LocationSort.StoreId.GetDescription(), LocationSort.StoreId },
            };
        }


        public LocationListModel(IEnumerable<Location> locations, LocationSort sort, bool desc)
        {
            this.Sort = sort;
            this.Desc = desc;

            locations = locations.ToList().AsEnumerable();
            if (this.Sort == LocationSort.LocationName)
            {
                locations = locations.OrderWithDirection(i => i.Name, desc);
            }

            if (this.Sort == LocationSort.StoreId)
            {
                locations = locations.OrderWithDirection(i => i.StoreId, desc);
            }

            if (this.Sort == LocationSort.LocationType)
            {
                locations = locations.OrderWithDirection(i => i.LocationTypeId, desc);
            }

            this.Locations = locations;
        }
    }
}
