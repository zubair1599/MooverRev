using System;
using System.Collections.Specialized;
using System.Linq;
using Business.Models;

namespace Business.Repository.Models
{
    public class ErrorRepository : RepositoryBase<Error>
    {
        private static readonly string[] IgnoreUrls = {
            "parseleads",
            "Home/IsAuthorized"
        };

        public override Error Get(Guid id)
        {
            return db.Errors.SingleOrDefault(e => e.ErrorLogID == id);
        }

        public Utility.PagedResult<Error> GetPage(int page, int take)
        {
            var errors = db.Errors.OrderByDescending(i => i.Created);
            return new Utility.PagedResult<Error>(errors, page, take);
        }

        public void Log(Exception e, string url)
        {
            var emptyserver = new NameValueCollection();
            var emptypost = new NameValueCollection();
            this.Log(e, url, emptyserver, emptypost);
        }

        public void Log(Exception e, string url, NameValueCollection serverVars, NameValueCollection postVars)
        {
            if (IgnoreUrls.Any(u => url.Contains(u)))
            {
                return;
            }

            var serverDictionary = serverVars.AllKeys.ToDictionary(key => key, key => serverVars[key]);
            var postData = postVars.AllKeys.ToDictionary(key => "PostKey -- " + key, val => postVars[val]);
            var allItems = postData.Concat(serverDictionary);

            var error = new Error {
                StackTrace = e.StackTrace,
                Message = e.Message,
                ServerVariables = Utility.LocalExtensions.SerializeToJson(allItems),
                URL = url,
                Created = DateTime.Now
            };
            this.Add(error);
        }
    }
}