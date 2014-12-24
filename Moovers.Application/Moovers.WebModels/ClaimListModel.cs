using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Business.Enums;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public sealed class ClaimListModel : SortableModel<ClaimFields>
    {
        public override ClaimFields Sort { get; set; }
        public Guid CaseId { get; set; }
        public string Caselookup { get; set; }
        public bool CaseSubmitStatus { get; set; }

        public override bool Desc { get; set; }

        public IEnumerable<Claim> Claim { get; set; }

        public List<Claim> ClaimList = new List<Claim>();
        public Business.Models.Claim CustomerClaim { get; set; }
        public SelectList InventoryItems {get ; set;}

        public override IEnumerable<KeyValuePair<string, ClaimFields>> GetHeaders()
        {
            return new Dictionary<string, ClaimFields>() {
                { ClaimFields.ClaimID.GetDescription(), ClaimFields.ClaimID },
                {ClaimFields.Inventory.GetDescription(), ClaimFields.Inventory},
                {ClaimFields.ClaimType.GetDescription(), ClaimFields.ClaimType},
                {ClaimFields.Remarks.GetDescription(), ClaimFields.Remarks},
               

               
            };
        }

        public ClaimListModel()
        {

        }
        public ClaimListModel(IEnumerable<Claim> Claims, ClaimFields sort, bool desc)
        {
            this.Sort = sort;
            this.Desc = desc;

            Claim = Claims.ToList().AsEnumerable();

            if (this.Sort == ClaimFields.Created)
            {
                Claims = Claims.OrderWithDirection(i => i.Created, desc);

            }

            this.Claim = Claims;
        }


        public static readonly IDictionary<string, string> StateDictionary = new Dictionary<string, string>
        {
            {ClaimType.Damage.GetDescription(),"1"},
            {ClaimType.Lost.GetDescription(), "2"},
        };

        public SelectList ClaimTypes
        {

            get
            {
                var claimtypelist = StateDictionary.ToDictionary(i => i.Key, i => i.Value);
                return new SelectList(claimtypelist, "Value", "Key");

            }
        }
    }
}
