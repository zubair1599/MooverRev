using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Reflection;
using System.Data.Objects;
using Business.Models;
using Business.Repository.Models;

namespace Business.Utility
{
    using Newtonsoft.Json;

    public static class General
    {
        public static string WebQuoteUser
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["WebQuoteUser"];
            }
        }
         //[JsonExtensionData]
        public static readonly IDictionary<string, string> StateDictionary = new Dictionary<string, string> {
            {"Alabama", "AL"},
            {"Alaska", "AK"},
            {"Arizona", "AZ"},
            {"Arkansas", "AR"},
            {"California", "CA"},
            {"Colorado", "CO"},
            {"Connecticut", "CT"},
            {"Delaware", "DE"},
            {"District of Columbia", "DC"},
            {"Florida", "FL"},
            {"Georgia", "GA"},
            {"Hawaii", "HI"},
            {"Idaho", "ID"},
            {"Illinois", "IL"},
            {"Indiana", "IN"},
            {"Iowa", "IA"},
            {"Kansas", "KS"},
            {"Kentucky", "KY"},
            {"Louisiana", "LA"},
            {"Maine", "ME"},
            {"Maryland", "MD"},
            {"Massachusetts", "MA"},
            {"Michigan", "MI"},
            {"Minnesota", "MN"},
            {"Mississippi", "MS"},
            {"Missouri", "MO"},
            {"Montana", "MT"},
            {"Nebraska", "NE"},
            {"Nevada", "NV"},
            {"New Hampshire", "NH"},
            {"New Jersey", "NJ"},
            {"New Mexico", "NM"},
            {"New York", "NY"},
            {"North Carolina", "NC"},
            {"North Dakota", "ND"},
            {"Ohio", "OH"},
            {"Oklahoma", "OK"},
            {"Oregon", "OR"},
            {"Pennsylvania", "PA"},
            {"Rhode Island", "RI"},
            {"South Carolina", "SC"},
            {"South Dakota", "SD"},
            {"Tennessee", "TN"},
            {"Texas", "TX"},
            {"Utah", "UT"},
            {"Vermont", "VT"},
            {"Virginia", "VA"},
            {"Washington", "WA"},
            {"West Virginia", "WV"},
            {"Wisconsin", "WI"},
            {"Wyoming", "WY"}
        };

        public static string GetStateAbbreviation(string fullstate)
        {
            fullstate = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(fullstate ?? String.Empty).Trim();
            if (StateDictionary.ContainsKey(fullstate))
            {
                return StateDictionary[fullstate];
            }

            return null;
        }

        public static SelectList SelectStateList
        {
            get
            {
                var dict = StateDictionary.ToDictionary(i => i.Value + " (" + i.Key + ")", i => i.Value);
                return new SelectList(dict, "Value", "Key");
            }
        }

        public static SelectList SelectStateListwithSelectedValue(object selectedValue = null)
        {

            var dict = StateDictionary.ToDictionary(i => i.Value + " (" + i.Key + ")", i => i.Value);
            return new SelectList(dict, "Value", "Key" , selectedValue);
        }

        public static decimal MetersToMiles(decimal meters)
        {
            return (meters / (decimal)1609.34);
        }

        public static dynamic Combine(dynamic item1, dynamic item2)
        {
            var dictionary1 = (IDictionary<string, object>)item1;
            var dictionary2 = (IDictionary<string, object>)item2;
            var result = new ExpandoObject();
            var d = result as IDictionary<string, object>;

            foreach (var pair in dictionary1.Concat(dictionary2))
            {
                d[pair.Key] = pair.Value;
            }

            return result;
        }

        private static readonly char[] ReadableCharacters = {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'j', 'k',
            'm', 'n', 'p', 'q', 'r', 's', 't', 'w', 'x', 'y',
            'z', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static string GetHourRange(decimal minutes)
        {
            var hours = minutes / 60;
            return String.Format("{0} - {1} Hours", Math.Floor(hours), Math.Ceiling(hours));
        }

        private static Random Rng = new Random();

        public static string RandomString(int length)
        {
            return RandomString(length, ReadableCharacters);
        }

        public static T DeserializeJson<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static byte[] GeneratePdf(string htmlstring, System.Drawing.Printing.PaperKind paperKind)
        {
            var converter = new Moovers.HtmlToPdf.PDFConverter();
            return converter.Convert(htmlstring, paperKind, Moovers.HtmlToPdf.PDFOrientation.Portrait);
        }

        public static string RandomString(int length, IList<char> characters)
        {
            lock (Rng)
            {
                var ret = new char[length];
                for (var i = 0; i < length; i++)
                {
                    ret[i] = characters[Rng.Next(characters.Count)];
                }

                return new string(ret);
            }
        }

        public static bool IsGuid(string test)
        {
            Guid tmp;
            var ret = Guid.TryParse(test, out tmp);
            return ret;
        }

        /// <summary>
        /// "user" searches a little bit differently than normal searches.
        /// A search for "taylor" will search for items with the name "taylor" in them
        /// A search for "*taylor" will search for items owned by the user with name "taylor"
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        public static Guid? GetAspUserFromSearch(string search)
        {
            if (!search.StartsWith("*") && search != "unassigned")
            {
                return null;
            }

            // "unassigned" just means quotes that haven't yet been claimed by a sales person.
            if (search == "unassigned")
            {
                search = WebQuoteUser;
            }
            else
            {
                search = search.Substring(1);
            }

            var item = new aspnet_UserRepository().Get(search.ToLower());
            if (item != null)
            {
                return item.UserId;
            }

            return null;
        }
    }
}
