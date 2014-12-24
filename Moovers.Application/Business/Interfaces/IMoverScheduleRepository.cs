using Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Interfaces
{
    public interface IMoverScheduleRepository
    {
        IEnumerable<Quote> GetScheduleForDay(Employee emp, int day, int month, int year);
    }
}
