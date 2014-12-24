using System.Collections.Generic;
using System.Linq;
using Business.Enums;
using Business.Interfaces;
using Business.Models;
using Business.Utility;

namespace Moovers.WebModels
{
    public class PostListModel : SortableModel<PostSortColumn?>
    {
        public string Page { get; set; }

        public Business.Utility.PagedResult<IPosting> Postings { get; set; }

        public override sealed PostSortColumn? Sort { get; set; }

        public override sealed bool Desc { get; set; }

        public IEnumerable<PostSortColumn> ColumnSkips { get; set; }

        public PostListModel(string page, Business.Utility.PagedResult<IPosting> postings, PostSortColumn sort, bool desc, IEnumerable<PostSortColumn> skips)
        {
            this.Page = page;
            this.Postings = postings;
            this.Sort = sort;
            this.ColumnSkips = skips;
            this.Desc = desc;
        }

        public override IEnumerable<KeyValuePair<string, PostSortColumn?>> GetHeaders()
        {
            var ret = new List<KeyValuePair<string, PostSortColumn?>>();

            if (!ColumnSkips.Contains(PostSortColumn.ServiceDate))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.ServiceDate.GetDescription(), PostSortColumn.ServiceDate));
            }

            if (!ColumnSkips.Contains(PostSortColumn.QuoteID))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.QuoteID.GetDescription(), PostSortColumn.QuoteID));
            }

            if (!ColumnSkips.Contains(PostSortColumn.PostingDate))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.PostingDate.GetDescription(), PostSortColumn.PostingDate));
            }

            if (!ColumnSkips.Contains(PostSortColumn.AccountName))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.AccountName.GetDescription(), PostSortColumn.AccountName));
            }

            if (!ColumnSkips.Contains(PostSortColumn.Price))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.Price.GetDescription(), PostSortColumn.Price));
            }

            if (!ColumnSkips.Contains(PostSortColumn.Employees))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.Employees.GetDescription(), PostSortColumn.Employees));
            }

            if (!ColumnSkips.Contains(PostSortColumn.Vehicles))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.Vehicles.GetDescription(), PostSortColumn.Vehicles));
            }

            if (!ColumnSkips.Contains(PostSortColumn.Balance))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.Balance.GetDescription(), PostSortColumn.Balance));
            }

            if (!ColumnSkips.Contains(PostSortColumn.Print))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.Print.GetDescription(), null));
            }

            if (!ColumnSkips.Contains(PostSortColumn.LostDebt))
            {
                ret.Add(new KeyValuePair<string, PostSortColumn?>(PostSortColumn.LostDebt.GetDescription(), null));
            }

            return ret;
        }
    }

    public class PostModel
    {
        public IEnumerable<Employee> Employees { get; set; }

        public Business.Models.Posting Posting { get; set; }

        public IEnumerable<Vehicle> Vehicles { get; set; }

        public PostModel(IEnumerable<Employee> employees, Business.Models.Posting posting, IEnumerable<Vehicle> vehicles)
        {
            this.Employees = employees;
            this.Posting = posting;
            this.Vehicles = vehicles;
        }
    }
}
