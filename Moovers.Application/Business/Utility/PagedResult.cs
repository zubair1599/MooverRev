using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Utility
{
    /// <summary>
    /// For use with "_PageList" - allows for easy pagination
    /// </summary>
    public abstract class PagedResult
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int PageCount
        {
            get
            {
                return (int)Math.Ceiling(TotalCount / (double)PageSize);
            }
        }

        public string GetPageDescription()
        {
            var min = Math.Min((this.CurrentPage * this.PageSize) + 1, this.TotalCount);
            var max = Math.Min(min + this.PageSize - 1, this.TotalCount);
            return String.Format("{0} - {1} of {2}", min, max, this.TotalCount);
        }
    }

    public class PagedResult<T> : PagedResult, IEnumerable<T>
    {
        public IEnumerable<T> Items { get; set; }

        public PagedResult()
        {
        }

        public PagedResult(IQueryable<T> allResults, int page, int pageSize)
        {
            this.PageSize = pageSize;
            this.CurrentPage = page;
            this.Items = allResults.Any() ? allResults.Skip(this.PageSize * this.CurrentPage).Take(this.PageSize).ToList() : allResults.AsEnumerable();
            this.TotalCount = allResults.Count();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }
    }
}
