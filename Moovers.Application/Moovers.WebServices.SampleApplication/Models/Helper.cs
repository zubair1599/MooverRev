using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace Moovers.WebServices.SampleApplication.Models
{
    public class Helper
    {
        public static Cookie CurrentCookie { get; set; }
        public static string CurrentToken { get; set; }

        public static string PrivateKey { get; set; }

        public static string UserId { get; set; }
        public static string MoveLookup { get; set; }
    }
}