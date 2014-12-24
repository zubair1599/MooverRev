using System;
using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public sealed class EmployeeListModel : SortableModel<EmployeeSort>
    {
        public override EmployeeSort Sort { get; set; }

        public override bool Desc { get; set; }

        public IEnumerable<Employee> Employees { get; set; }

        public bool OldHeaders { get; set; }

        public override IEnumerable<KeyValuePair<string, EmployeeSort>> GetHeaders()
        {
            return OldHeaders ? GetOldHeaders() : GetNewHeaders();
        }

        private IEnumerable<KeyValuePair<string, EmployeeSort>> GetOldHeaders()
        {
            return new Dictionary<string, EmployeeSort>() {
                { EmployeeSort.Number.GetDescription(), EmployeeSort.Number },
                { EmployeeSort.Name.GetDescription(), EmployeeSort.Name },
                { EmployeeSort.Position.GetDescription(), EmployeeSort.Position },
                { EmployeeSort.IsDriver.GetDescription(), EmployeeSort.IsDriver },
                { EmployeeSort.Wage.GetDescription(), EmployeeSort.Wage },
                { EmployeeSort.PrimaryPhone.GetDescription(), EmployeeSort.PrimaryPhone },
                { EmployeeSort.SecondaryPhone.GetDescription(), EmployeeSort.SecondaryPhone }
            };
        }

        private IEnumerable<KeyValuePair<string, EmployeeSort>> GetNewHeaders()
        {
            return new Dictionary<string, EmployeeSort>() {
                { EmployeeSort.Status.GetDescription(), EmployeeSort.Status },
                { EmployeeSort.EmployeeID.GetDescription(), EmployeeSort.EmployeeID },
                { EmployeeSort.Name.GetDescription(), EmployeeSort.Name },
                { EmployeeSort.Type.GetDescription(), EmployeeSort.Type },
                { EmployeeSort.Class.GetDescription(), EmployeeSort.Class },
                { EmployeeSort.YTD.GetDescription(), EmployeeSort.YTD },
                { EmployeeSort.LastWorked.GetDescription(), EmployeeSort.LastWorked },
                { EmployeeSort.Store.GetDescription(), EmployeeSort.Store },
                { EmployeeSort.JobTitle.GetDescription(), EmployeeSort.JobTitle }
            };
        }

        public EmployeeListModel(IEnumerable<Employee> emps, EmployeeSort sort, bool desc, bool oldheaders = true)
        {
            this.Sort = sort;
            this.Desc = desc;
            this.OldHeaders = oldheaders;

            emps = emps.ToList().AsEnumerable();
            if (this.Sort == EmployeeSort.Name)
            {
                emps = emps.OrderWithDirection(i => i.DisplayName(), desc);
            }

            if (this.Sort == EmployeeSort.Number)
            {
                emps = emps.OrderWithDirection(i => int.Parse(i.Lookup), desc);
            }

            if (this.Sort == EmployeeSort.Position)
            {
                emps = emps.OrderWithDirection(i => i.PositionType.GetDescription(), desc);
            }

            if (this.Sort == EmployeeSort.EmployeeID)
            {
                emps = emps.OrderWithDirection(i => i.EmployeeNumber, desc);
            }

            if (this.Sort == EmployeeSort.IsDriver)
            {
                emps = emps.OrderWithDirection(i => i.PositionType == Position.Driver, !desc);
            }

            if (this.Sort == EmployeeSort.JobTitle)
            {
                emps = emps.OrderWithDirection(i => i.PositionType.GetDescription(), desc);
            }

            if (this.Sort == EmployeeSort.Type)
            {
                emps = emps.OrderWithDirection(i => i.TypeId, desc);
            }

            if (this.Sort == EmployeeSort.Class)
            {
                emps = emps.OrderWithDirection(i => i.ClassId, desc);
            }

            if (this.Sort == EmployeeSort.Store)
            {
                emps = emps.OrderWithDirection(i => i.Franchise.StoreNumber, desc);
            }

            if (this.Sort == EmployeeSort.Wage)
            {
                emps = emps.OrderWithDirection(i => i.Wage, desc);
            }

            if (this.Sort == EmployeeSort.PrimaryPhone)
            {
                emps = emps.OrderWithDirection(i => i.PrimaryPhone != null ? i.PrimaryPhone.DisplayString() : String.Empty, desc);
            }

            if (this.Sort == EmployeeSort.SecondaryPhone)
            {
                emps = emps.OrderWithDirection(i => i.SecondaryPhone != null ? i.SecondaryPhone.DisplayString() : String.Empty, desc);
            }

            this.Employees = emps;
        }
    }
}
