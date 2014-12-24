using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Models;
using Business.Repository.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public class QuoteListModel : SortableModel<QuoteSort?>
    {
        public PagedResult<Quote> Quotes { get; set; }

        public override sealed QuoteSort? Sort { get; set; }

        public override sealed bool Desc { get; set; }

        public QuoteListModel(PagedResult<Quote> quotes, QuoteSort sort, bool desc)
        {
            this.Quotes = quotes;
            this.Sort = sort;
            this.Desc = desc;
        }

        public IEnumerable<aspnet_User> GetUsers()
        {
            var repo = new aspnet_UserRepository();
            return repo.GetForFilter().OrderBy(i => i.UserName);
        }

        public override IEnumerable<KeyValuePair<string, QuoteSort?>> GetHeaders()
        {
            var objs = new[] {
                QuoteSort.QuoteID,
                QuoteSort.Account,
                QuoteSort.Date,
                QuoteSort.DaysToMove,
                QuoteSort.Price,
                QuoteSort.Type,
                QuoteSort.SalesID,
                QuoteSort.VisualSurvey,
                QuoteSort.LastModified,
                QuoteSort.Status
            };

            var list = objs.Select(i => new KeyValuePair<string, QuoteSort?>(i.GetDescription(), i)).ToList();
            list.Add(new KeyValuePair<string, QuoteSort?>("Action", null));
            return list;
        }

        public object ToJsonObject()
        {
            return new {
                Quotes = this.Quotes.Select(i => i.ToListingJsonObject()),
                Sort = this.Sort,
                Desc = this.Desc,
                PagedResult = new {
                    CurrentPage = this.Quotes.CurrentPage,
                    PageSize = this.Quotes.PageSize,
                    TotalCount = this.Quotes.TotalCount,
                    PageCount = this.Quotes.PageCount,
                    Description = this.Quotes.GetPageDescription()
                }
            };
        }
    }
}
