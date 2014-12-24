using Business.Interfaces;
using Business.Models;
using System.Collections.Generic;
using System.Linq;

namespace Business.Repository
{
    public class MoverScheduleRepository : RepositoryBase, IMoverScheduleRepository
    {
        public IEnumerable<Quote> GetScheduleForDay(Employee emp, int day, int month, int year)
        {
            var crews = (from crew in db.Crews
                where crew.Day == day
                      && crew.Month == month
                      && crew.Year == year
                      && crew.FranchiseID == emp.FranchiseID
                select crew);

            var thisCrews =
                crews.Where(i => i.Crew_Employee_Rel.Any(r => r.EmployeeID == emp.EmployeeID && !r.IsArchived));
          
            return thisCrews.SelectMany(
                c => c.Crew_Schedule_Rel.Where(cs => !cs.Schedule.IsCancelled).Select(ce => ce.Schedule).Select(q=>q.Quote));

        }
    }
}
