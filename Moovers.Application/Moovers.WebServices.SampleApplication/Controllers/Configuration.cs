// // --------------------------------------------------------------------------------------------------------------------
// // <copyright file="Configuration.cs" company="Moovers Inc.">
// //   Copyright (c) Moovers Inc. All rights reserved.
// // </copyright>
// // --------------------------------------------------------------------------------------------------------------------

namespace Moovers.WebServices.SampleApplication.Controllers
{
    using System.Configuration;

    public static class Configuration
    {
        public static string UsernameHeader
        {
            get { return ConfigurationManager.AppSettings["UsernameHeader"]; }
        }

        public static string AuthorizationMethod
        {
            get { return ConfigurationManager.AppSettings["Authorization"]; }
        }

        public static string ApiBaseUrl
        {
            get { return ConfigurationManager.AppSettings["ApiUrl"]; }
        }
    }
}