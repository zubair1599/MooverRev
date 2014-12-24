using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Business.Models;
using Business.Utility;

namespace Business.Repository.Models
{
    public enum VehicleSort
    {
        Lookup,
        Name,
        Year,
        Franchise,
        CubicFeet,
        Length,
        Model,
        Make
    }

    public class VehicleRepository : RepositoryBase<Vehicle>
    {
        private static readonly Func<MooversCRMEntities, Guid, Vehicle> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, Vehicle>(
            (db, id) => db.Vehicles.SingleOrDefault(i => i.VehicleID == id)
            );

        private static readonly Func<MooversCRMEntities, string, Vehicle> CompiledGetLookup = CompiledQuery.Compile<MooversCRMEntities, string, Vehicle>(
            (db, lookup) => db.Vehicles.SingleOrDefault(i => i.Lookup == lookup)
            );

        public override Vehicle Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public Vehicle Get(string lookup)
        {
            return CompiledGetLookup(db, lookup);
        }

        public IEnumerable<Vehicle> GetAll(VehicleSort sort = VehicleSort.Lookup, bool desc = true)
        {
            var ret = (from v in db.Vehicles
                where !v.IsArchived
                select v);

            if (sort == VehicleSort.Name)
            {
                ret = ret.OrderWithDirection(i => i.Name, desc);
            }

            if (sort == VehicleSort.Lookup)
            {
                ret = ret.OrderWithPadding(i => i.Lookup, 12, desc);
            }

            if (sort == VehicleSort.Year)
            {
                ret = ret.OrderWithDirection(i => i.Year, desc);
            }

            if (sort == VehicleSort.CubicFeet)
            {
                ret = ret.OrderWithDirection(i => i.CubicFeet, desc);
            }

            if (sort == VehicleSort.Length)
            {
                ret = ret.OrderWithDirection(i => i.Length, desc);
            }

            if (sort == VehicleSort.Make)
            {
                ret = ret.OrderWithDirection(i => i.Make, desc);
            }

            if (sort == VehicleSort.Model)
            {
                ret = ret.OrderWithDirection(i => i.Model, desc);
            }

            return ret;
        }

        public IEnumerable<Vehicle> GetAll(Guid franchiseID, VehicleSort sort = VehicleSort.Lookup, bool desc = true)
        {
            return this.GetAll(sort, desc).Where(i => i.FranchiseID == franchiseID);
        }
    }
}