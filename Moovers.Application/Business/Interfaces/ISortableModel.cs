using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Business.Interfaces
{
    public interface ISortableModel
    {
        object Sort { get; set; }

        bool Desc { get; set; }

        IEnumerable<KeyValuePair<string, object>> GetHeaders();
    }

    public interface ISortableModel<T> : ISortableModel
    {
        new T Sort { get; set; }

        new bool Desc { get; set; }

        new IEnumerable<KeyValuePair<string, T>> GetHeaders();
    }
}
