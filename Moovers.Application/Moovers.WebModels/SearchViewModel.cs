using System.Collections.Generic;
using System.ComponentModel;
using Business.Interfaces;
using Business.Utility;

namespace Moovers.WebModels
{
    public enum SearchResultSort
    {
        [Description("ID")]
        ID,

        [Description("Account")]
        Account,

        [Description("Price")]
        Price,

        [Description("Open Date")]
        Opened,

        [Description("Quote Manager")]
        QuoteManager,

        [Description("Status")]
        Status
    }

    public sealed class SearchViewModel : SortableModel<SearchResultSort>
    {
        public override SearchResultSort Sort { get; set; }

        public override bool Desc { get; set;  }

        private IEnumerable<ISearchResult> SearchResults { get; set; }

        public override IEnumerable<KeyValuePair<string, SearchResultSort>> GetHeaders()
        {
            var ret = new Dictionary<string, SearchResultSort> {
                { SearchResultSort.ID.GetDescription(), SearchResultSort.ID }, 
                { SearchResultSort.Account.GetDescription(), SearchResultSort.Account }, 
                { SearchResultSort.Price.GetDescription(), SearchResultSort.Price }, 
                { SearchResultSort.Opened.GetDescription(), SearchResultSort.Opened }, 
                { SearchResultSort.QuoteManager.GetDescription(), SearchResultSort.QuoteManager }, 
                { SearchResultSort.Status.GetDescription(), SearchResultSort.Status }
            };
            return ret;
        }

        public SearchViewModel(IEnumerable<ISearchResult> results,  bool desc = false, SearchResultSort sort = SearchResultSort.Opened)
        {
            this.SearchResults = results;
            this.Desc = desc;
            this.Sort = sort;
        }
    }
}
