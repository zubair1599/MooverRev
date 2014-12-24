using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Business.Models;
using System.Dynamic;
using Business.ToClean.QuoteHelpers;
using LinqKit;

namespace Business.Utility
{
    public static class LocalExtensions
    {
        /// <summary>
        /// Gets the System.ComponentModel.Description attribute for an enum
        ///
        /// ex:
        /// enum Sample {
        ///     // returns "First Sample"
        ///     [System.ComponentModel.Description("First Sample")]
        ///     FirstSample,
        ///     
        ///     // returns "SecondSample"
        ///     SecondSample
        /// }
        /// </summary>
        public static string GetDescription<T>(this T item)
            where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T Must be an enum");
            }

            var name = item.ToString();
            var memInfo = item.GetType().GetMember(name);
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Any())
            {
                name = ((DescriptionAttribute)attributes[0]).Description;
            }

            return name;
        }

        /// <summary>
        /// Returns a javascript dictionary of a enum
        /// </summary>
        public static string ToJavascriptObject<T>(this T enumtype)
            where T : struct, IConvertible
        {
            var sb = new StringBuilder();

            sb.Append("{");

            int count = 0;
            var dict = enumtype.ToDictionary();
            foreach (var kvp in dict)
            {
                count++;
                sb.Append("\"" + kvp.Key + "\" : \"" + kvp.Value + "\"");
                if (count != dict.Count())
                {
                    sb.Append(",");
                }
            }

            sb.Append("}");
            return sb.ToString();
        }

        public static IDictionary<string, string> ToDictionary<T>(this T enumtype, string optGroup)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T Must be an enum");
            }

            var ret = new Dictionary<string, string>();

            foreach (T i in Enum.GetValues(typeof(T)).Cast<T>())
            {
                var name = i.ToString();
                var memInfo = typeof(T).GetMember(name);
                var optGroupAttribute = memInfo[0].GetCustomAttributes(typeof(OptGroupAttribute), false);
                if (!String.IsNullOrEmpty(optGroup))
                {
                    if (optGroupAttribute.All(og => ((OptGroupAttribute)og).Name != optGroup.Trim()))
                    {
                        continue;
                    }
                }

                ret.Add(i.ToString(), i.GetDescription());
            }

            return ret;
        }

        public static IDictionary<string, string> ToDictionary<T>(this T enumtype)
            where T : struct, IConvertible
        {
            return enumtype.ToDictionary(null);
        }

        public static SelectList ToSelectList<T>(this T enumtype) 
            where T : struct, IConvertible
        {
            return new SelectList(enumtype.ToDictionary(), "Key", "Value", enumtype.ToString());
        }

        public static string SerializeToJson(this object o)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings() {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error,
                Formatting = Newtonsoft.Json.Formatting.None,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local
            });

            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, o);
                return sw.ToString();
            }
        }
        public static string SerializeToJson(this object o, int depth)
        {
            var serializer = Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error,
                Formatting = Newtonsoft.Json.Formatting.None,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Local,
                MaxDepth = depth
            });

            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, o);
                return sw.ToString();
            }
        }

        public static string Capitalize(this string str)
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }

            return str.Substring(0, 1).ToUpper() + str.Substring(1);
        }         
        
        public static IOrderedEnumerable<T> OrderWithDirection<T, TOrder>(this IEnumerable<T> source, Func<T, TOrder> ordering, bool descending)
        {
            if (descending)
            {
                return source.OrderByDescending(ordering);
            }

            return source.OrderBy(ordering);
        }

        public static IOrderedQueryable<T> ThenWithDirection<T, TOrder>(this IOrderedQueryable<T> source, Expression<Func<T, TOrder>> ordering, bool desc)
        {
            if (desc)
            {
                return source.ThenByDescending(ordering);
            }

            return source.ThenBy(ordering);
        }

        public static IOrderedQueryable<T> OrderWithDirection<T, TOrder>(this IQueryable<T> source, Expression<Func<T, TOrder>> ordering, bool descending)
        {
            if (descending)
            {
                return source.OrderByDescending(ordering);
            }

            return source.OrderBy(ordering);
        }

        // TODO: This is a very hacky padleft implementation for sorting purposes. It'd be nice to use the native SQL PadLeft function for this.
        public static IOrderedQueryable<T> OrderWithPadding<T>(this IQueryable<T> source, Expression<Func<T, string>> expr, int paddingLength, bool desc)
        {
            if (!source.Any())
            {
                return Enumerable.Empty<T>().AsQueryable().OrderBy(i => i);
            }

            var tmp = source.AsExpandable().Select(i => new {
                item = i,
                str = expr.Invoke(i)
            });

            var padding = new String('0', paddingLength + tmp.Max(i => i.str.Length));
            return (IOrderedQueryable<T>)tmp.OrderWithDirection(i => (padding + i.str).Substring((padding + i.str).Length - paddingLength, paddingLength), desc).Select(i => i.item);
        }

        public static IEnumerable<T> OrderWithPadding<T>(this IEnumerable<T> source, Func<T, string> expr, int paddingLength, bool desc)
        {
            if (!source.Any())
            {
                return Enumerable.Empty<T>();
            }

            var tmp = source.Select(i => new {
                item = i,
                str = expr.Invoke(i)
            });

            var padding = new String('0', paddingLength + tmp.Max(i => i.str.Length));
            return tmp.OrderWithDirection(i => (padding + i.str).Substring((padding + i.str).Length - paddingLength, paddingLength), desc).Select(i => i.item);
        }

        /// <summary>
        /// http://stackoverflow.com/questions/2554696/ef-4-removing-child-object-from-collection-does-not-delete-it-why
        /// "Removing" an item doesn't delete the item
        /// </summary>
        public static void Delete<T>(this EntityCollection<T> collection, T entityToDelete) 
            where T : EntityObject, IEntityWithRelationships
        {
            RelationshipManager relationshipManager = entityToDelete.RelationshipManager;

            IRelatedEnd relatedEnd = relationshipManager.GetAllRelatedEnds().FirstOrDefault();
            if (relatedEnd == null)
            {
                throw new Exception("No relationships found for the entity to delete. Entity must have at least one relationship.");
            }

            var query = relatedEnd.CreateSourceQuery() as ObjectQuery;
            if (query == null)
            {
                throw new Exception("The entity to delete is detached. Entity must be attached to an ObjectContext.");
            }

            query.Context.DeleteObject(entityToDelete);
            collection.Remove(entityToDelete);
        }

        /// <summary>
        ///  "Empty" doesn't work on collection, this is an extension in the same vein.
        /// </summary>
        public static void DeleteAll<T>(this EntityCollection<T> collection) 
            where T : EntityObject, IEntityWithRelationships
        {
            collection.DeleteAll(i => true);
        }

        public static void DeleteAll<T>(this EntityCollection<T> collection, Func<T, bool> expr)
            where T : EntityObject, IEntityWithRelationships
        {
            var list = collection.Where(expr).ToList();
            foreach (var item in list)
            {
                collection.Delete(item);
            }
        }
    }
}