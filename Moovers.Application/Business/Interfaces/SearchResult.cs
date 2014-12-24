using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Interfaces
{
    public enum SearchType
    {
        Quote,
        Account,
        Storage,
        Lead
    }

    public interface ISearchResult
    {
        string DisplayID { get; }

        string DisplayAccount { get; }

        DateTime? OpenDate { get; }

        decimal? Amount { get; }

        SearchType Type { get; }

        string Status { get; }
    }
}
