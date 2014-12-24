using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Utility
{
    /// <summary>
    /// This base class works with "Views/Shared/_SortableTableHeader
    /// </summary>
    /// <typeparam name="T">
    /// Enum or Nullable enum to sort by
    /// </typeparam>
    public abstract class SortableModel<T> : Interfaces.ISortableModel<T>
    {
        /// <summary>
        /// Current sort
        /// </summary>
        public abstract T Sort { get; set; }

        /// <summary>
        /// Ascending/Descending
        /// </summary>
        public abstract bool Desc { get; set; }

        /// <summary>
        /// Ordered KeyValuePairs of SortType/display name
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<KeyValuePair<string, T>> GetHeaders();

        // Untyped members -- these are used so SortableTableHeader doesn't need type constraints
        object Interfaces.ISortableModel.Sort
        {
            get { return this.Sort; }
            set { this.Sort = (T)value; }
        }

        IEnumerable<KeyValuePair<string, object>> Interfaces.ISortableModel.GetHeaders()
        {
            return this.GetHeaders().Select(i => new KeyValuePair<string, object>(i.Key, i.Value));
        }
    }
}
