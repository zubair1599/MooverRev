using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.JsonObjects
{
    public sealed class SmallAccountJson : Interfaces.ISearchResult
    {
        public string Name { get; set; }

        public Guid AccountID { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Lookup { get; set; }

        public string Type { get; set; }

        string Interfaces.ISearchResult.DisplayID
        {
            get { return this.Lookup; }
        }

        string Interfaces.ISearchResult.DisplayAccount
        {
            get { return this.Name; }
        }

        DateTime? Interfaces.ISearchResult.OpenDate
        {
            get { return null; }
        }

        decimal? Interfaces.ISearchResult.Amount
        {
            get { return null; }
        }

        Interfaces.SearchType Interfaces.ISearchResult.Type
        {
            get { return Interfaces.SearchType.Account; }
        }

        string Interfaces.ISearchResult.Status
        {
            get { return String.Empty; }
        }
    }
}
