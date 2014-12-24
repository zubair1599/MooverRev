// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="SmartMoveRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository
{
    using System;
    using System.Data.Objects;
    using System.Linq;

    using Business.Models;

    public class SmartMoveRepository : RepositoryBase<SmartMove>
    {
        private static Func<MooversCRMEntities, Guid, SmartMove> CompiledGetByID = CompiledQuery.Compile<MooversCRMEntities, Guid, SmartMove>(
            (db, id) => db.SmartMoves.SingleOrDefault(i => i.SmartMoveID == id));

        private static Func<MooversCRMEntities, string, SmartMove> CompiledGetByLookup = CompiledQuery.Compile<MooversCRMEntities, string, SmartMove>(
            (db, lookup) => db.SmartMoves.SingleOrDefault(i => i.SmartMoveLookUp == lookup));


        public override SmartMove Get(Guid id)
        {
            return CompiledGetByID(db, id);
        }

        public SmartMove Get(string lookup)
        {
            return CompiledGetByLookup(db, lookup);
        }

        public IQueryable<SmartMove> GetAll()
        {
            return (from l in db.SmartMoves
                    where !l.IsArchived
                    orderby l.AddedDate descending
                    select l);
        }

        public IQueryable<SmartMove> GetTodayLeads()
        {
            return (from l in db.SmartMoves
                    where l.AddedDate.Day == DateTime.Now.Day 
                    && l.AddedDate.Month == DateTime.Now.Month 
                    && l.AddedDate.Year == DateTime.Now.Year
                    select l);
        }
    }
}