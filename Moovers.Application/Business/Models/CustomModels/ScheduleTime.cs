using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Models.CustomModels
{
    public class ScheduleTime
    {
        public  ScheduleTime(DateTime date,int startTime, int endTime)
        {
            var start = new TimeSpan(0,startTime,0,0);
            this.start_time = new DateTime(date.Year,date.Month,date.Day,start.Hours,start.Minutes,start.Seconds).ToUniversalTime();
            var end = new TimeSpan(0, endTime, 0, 0);
            this.end_time = new DateTime(date.Year, date.Month, date.Day, end.Hours, end.Minutes, end.Seconds).ToUniversalTime();
        }
        public DateTime  start_time {get; set; }
        public DateTime end_time { get; set; }
        public decimal downtime { get; set; }

        public string downtime_message { get; set; }
    }
}
