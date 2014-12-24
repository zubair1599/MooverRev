// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ScheduleNoteRepository.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Business.Repository.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Objects;
    using System.Linq;

    using Business.Models;

    public class ScheduleNoteRepository : RepositoryBase<ScheduleNote>
    {
        /// <summary>
        ///     Holidays/schedule notes that are the same every year
        /// </summary>
        private readonly Dictionary<KeyValuePair<int, int>, string> permanentHolidays = new Dictionary<KeyValuePair<int, int>, string>()
        {
            { new KeyValuePair<int, int>(12, 31), "New Year's Eve" },
            { new KeyValuePair<int, int>(1, 1), "New Year's Day" },
            { new KeyValuePair<int, int>(12, 25), "CHRISTMAS DAY" },
            { new KeyValuePair<int, int>(12, 24), "CHRISTMAS EVE" },
            { new KeyValuePair<int, int>(7, 4), "INDEPENDENCE DAY" },
        };

        private readonly Func<MooversCRMEntities, int, int, int, ScheduleNote> compiledGetByDate =
            CompiledQuery.Compile<MooversCRMEntities, int, int, int, ScheduleNote>(
                (db, day, month, year) => db.ScheduleNotes.FirstOrDefault(i => i.Day == day && i.Month == month && i.Year == year));

        public IEnumerable<ScheduleNote> GetAll()
        {
            return db.ScheduleNotes;
        }

        public override ScheduleNote Get(Guid id)
        {
            return db.ScheduleNotes.SingleOrDefault(i => i.NoteID == id);
        }

        public void Remove(Guid id)
        {
            db.ScheduleNotes.DeleteObject(Get(id));
        }

        public override void Save(ApplicationType applicationType = ApplicationType.Crm)
        {
            // schedule notes are saved in app-cache - we need to clear the cache when we update the repo
            var cacheRepo = new CacheRepository();
            cacheRepo.Clear("Note-");
            base.Save(applicationType);
        }

        public IEnumerable<ScheduleNote> GetBetween(DateTime start, DateTime end)
        {
            var items = new List<DateTime>();
            for (DateTime i = start.Date; i <= end.Date; i = i.AddDays(1))
            {
                items.Add(i);
            }

            List<ScheduleNote> notes = (from date in items let note = GetForDay(date) where note != null select GetForDay(date)).ToList();

            return notes;
        }

        private ScheduleNote GetForDay(DateTime date)
        {
            int day = date.Day;
            int month = date.Month;
            int year = date.Year;

            KeyValuePair<KeyValuePair<int, int>, string> permanent = permanentHolidays.FirstOrDefault(i => i.Key.Key == month && i.Key.Value == day);
            if (!permanent.Equals(default(KeyValuePair<KeyValuePair<int, int>, string>)))
            {
                return new ScheduleNote() { Day = day, Month = month, Year = year, Message = permanent.Value, NoteID = Guid.Empty };
            }

            var cacheRepo = new CacheRepository();
            string cacheKey = "Note-" + day + month + year;
            if (!cacheRepo.Contains(cacheKey))
            {
                ScheduleNote note = compiledGetByDate(db, day, month, year);
                string msg = note != null ? note.Message : String.Empty;
                cacheRepo.Store(cacheKey, msg);
            }

            var cached = cacheRepo.Get<string>(cacheKey);
            if (String.IsNullOrEmpty(cacheKey))
            {
                return default(ScheduleNote);
            }

            return new ScheduleNote() { Day = day, Month = month, Year = year, Message = cached, NoteID = Guid.Empty };
        }
    }
}