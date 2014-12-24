using System;
using System.Collections;
using System.Linq;
using System.Web;

namespace Business.Repository.Models
{
    /// <summary>
    /// A wrapper around HttpRuntime cache
    /// </summary>
    public class CacheRepository : ICacheRepository
    {
        public void Store(string key, object value)
        {
            HttpRuntime.Cache.Insert(key, value);
        }

        public bool Contains(string key)
        {
            return HttpRuntime.Cache[key] != null;
        }

        public object Get(string key)
        {
            return HttpRuntime.Cache[key];
        }

        public void Clear(string containing = null)
        {
            containing = (containing ?? String.Empty);
            var keys = (from key in HttpRuntime.Cache.Cast<DictionaryEntry>().Select(k => k.Key.ToString())
                        where key.Contains(containing)
                        select key).ToList();

            foreach (var key in keys)
            {
                HttpRuntime.Cache.Remove(key);
            }
        }

        public T Get<T>(string key)
        {
            var val = this.Get(key);
            if (val == null)
            {
                return default(T);
            }

            return (T)Get(key);
        }
    }
}
