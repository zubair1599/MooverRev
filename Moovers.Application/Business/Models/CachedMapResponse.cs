using System;
using System.Collections.Generic;
using System.Text;
using Business;

namespace Business.Models
{
    /// <summary>
    /// Cache object for external Google Maps directions
    /// </summary>
    public partial class CachedMapResponse
    {
        public CachedMapResponse()
        {
            this.Date = DateTime.Now;
        }

        public CachedMapResponse(string response)
            : this()
        {
            this.Response = response;
        }
    }
}