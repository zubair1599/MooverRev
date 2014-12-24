using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.Utility
{
    public static class ContentTypes
    {
        public static readonly Dictionary<string, string> ExtensionList = new Dictionary<string, string>() 
        {
            { "application/msword", ".doc" },
            { "application/x-gzip", ".gzip" },
            { "multipart/x-gzip", ".gzip" },
            { "image/jpeg", ".jpg" },
            { "video/quicktime", ".mov" },
            { "application/pdf", ".pdf" },
            { "image/png", ".png" },
            { "application/mspowerpoint", ".ppt" },
            { "application/powerpoint", ".ppt" },
            { "application/vnd.ms-powerpoint", ".ppt" },
            { "application/x-mspowerpoint", ".ppt" },
            { "application/rtf", ".rtf" },
            { "application/x-rtf", ".rtf" },
            { "text/html", ".shtml" },
            { "text/x-server-parsed-html", ".shtml" },
            { "application/x-shockwave-flash", ".swf" },
            { "text/plain", ".txt" },
            { "application/excel", ".xls" },
            { "application/vnd.ms-excel", ".xls" },
            { "application/x-excel", ".xls" },
            { "application/x-msexcel", ".xls" },
            { "application/xml", ".xml" },
            { "text/xml", ".xml" },
            { "application/x-compressed", ".zip" },
            { "application/x-zip-compressed", ".zip" },
            { "application/zip", ".zip" },
            { "multipart/x-zip", ".zip" },
        };
    }
}