using System;
using System.Collections.Generic;
using System.Text;
using Business.Repository.Models;

namespace Business.Models
{
    public sealed partial class Lead : Interfaces.ISearchResult
    {
        public JsonObjects.LeadJson GetLeadJson()
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<JsonObjects.LeadJson>(this.LeadJson);
        }

        string Interfaces.ISearchResult.DisplayID
        {
            get { return this.LeadID.ToString(); }
        }

        string Interfaces.ISearchResult.DisplayAccount
        {
            get { return this.Name; }
        }

        DateTime? Interfaces.ISearchResult.OpenDate
        {
            get { return this.AddedDate;  }
        }

        decimal? Interfaces.ISearchResult.Amount
        {
            get { return null; }
        }

        Interfaces.SearchType Interfaces.ISearchResult.Type
        {
            get { return Interfaces.SearchType.Lead; }
        }

        string Interfaces.ISearchResult.Status
        {
            get 
            {
                if (this.IsArchived)
                {
                    return "Archived";
                }

                return String.Empty;
            }
        }
        public string DisplayOwner
        {
            get
            {
                return this.AccountManagerID!=null? new aspnet_Users_ProfileRepository().Get((Guid)this.AccountManagerID).DisplayName(): string.Empty;
            }

        }
        public object ToJsonObject()
        {
            return new
            {
                LeadID = this.LeadID,
                Email = this.Email,
                Name = this._Name,
                AddedDate = this.AddedDate.ToShortDateString(),
                Source = this.Source,
                Franchise = this.Franchise.Name,
                Owner = new aspnet_Users_ProfileRepository().Get((Guid)this.AccountManagerID).DisplayName()
            };
        }
    }
}
