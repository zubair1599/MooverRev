// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="ScheduleRepresentation.cs" company="Moovers Inc">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace WebServiceModels
{
    using System.Collections.Generic;
    using System.Linq;

    using Business.Models;
    using Business.Models.CustomModels;

    using WebGrease.Css.Extensions;

    public class ScheduleRepresentation
    {
        public ScheduleRepresentation(Schedule schedule)
        {
            schedule_id = schedule.ScheduleID.ToString();
            estimated_time = new ScheduleTime(schedule.Date, schedule.StartTime, schedule.EndTime);
            note = schedule.Note;
            crew = new List<CrewRepresentation>();
            schedule.GetCrews().ForEach(c => crew.Add(new CrewRepresentation(c)));
            Stop start = schedule.Quote.GetStops().FirstOrDefault();
            if (start != null)
            {
                start_stop_id = start.StopID.ToString();
            }
            Stop end = schedule.Quote.GetStops().LastOrDefault();
            if (end != null)
            {
                start_stop_id = end.StopID.ToString();
            }
        }

        public ScheduleRepresentation()
        {
        }

        public ScheduleTime estimated_time { get; set; }

        public ScheduleTime actual_time { get; set; }

        public string note { get; set; }

        public string start_stop_id { get; set; }

        public string end_stop_id { get; set; }

        public List<CrewRepresentation> crew { get; set; }

        public string schedule_id { get; set; }
    }
}