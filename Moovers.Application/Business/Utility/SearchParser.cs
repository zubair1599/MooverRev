using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Business.Utility
{
    public enum SearchTokenType
    {
        Query,
        And,
        Or
    }

    public class SearchToken
    {
        public SearchTokenType TokenType { get; set; }

        public string Text { get; set; }
    }

    public class SearchParser
    {
        public List<SearchToken> Tokens { get; set; }

        public static SearchParser Parse(string search)
        {
            search = (search ?? String.Empty).Trim();
            var ret = new SearchParser();
            var pattern = @"( AND )|( OR )";

            var tokens = Regex.Split(search, pattern, RegexOptions.IgnoreCase);

            ret.Tokens = tokens.Select(i => new SearchToken() {
                TokenType = (i.ToLower() == " and ") ? SearchTokenType.And
                            : (i.ToLower() == " or ") ? SearchTokenType.Or
                            : SearchTokenType.Query,
                Text = i
            }).ToList();

            return ret;
        }

        public SearchParser()
        {
            this.Tokens = new List<SearchToken>();
        }
    }
}
