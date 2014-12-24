using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public sealed class CaseListModel : SortableModel<CaseSort>
    {
        public override CaseSort Sort { get; set; }

        public override bool Desc { get; set; }

        public IEnumerable<Case> Case { get; set; }

        public override IEnumerable<KeyValuePair<string, CaseSort>> GetHeaders()
        {
            return new Dictionary<string, CaseSort>() {
                { CaseSort.Status.GetDescription(), CaseSort.Status },
                {CaseSort.CaseID.GetDescription(), CaseSort.CaseID},
                {CaseSort.Shipper.GetDescription(), CaseSort.Shipper},
                {CaseSort.Coverage.GetDescription(), CaseSort.Coverage},
                {CaseSort.DaysOpen.GetDescription(), CaseSort.DaysOpen},
                {CaseSort.Created.GetDescription(), CaseSort.Created},
                {CaseSort.Priority.GetDescription(), CaseSort.Priority},
                {CaseSort.Updated.GetDescription(), CaseSort.Updated},
            };
        }

        public CaseListModel(IEnumerable<Case> cases, CaseSort sort, bool desc)
        {
            this.Sort = sort;
            this.Desc = desc;

            Case = cases.ToList().AsEnumerable();

            if (this.Sort == CaseSort.Status)
            {
                cases = cases.OrderWithDirection(i => i.Status, desc);

            }

            this.Case = cases;
        }
    }
}
