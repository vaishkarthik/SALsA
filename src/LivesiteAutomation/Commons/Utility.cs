using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public static class Utility
    {
        public static T JsonToObject<T>(string json)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static string ObjectToJson(object obj, bool beautify = false)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                Formatting = beautify == false ? Formatting.None : Formatting.Indented
            };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string DecodeHtml(string html)
        {
            var regex = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            return WebUtility.HtmlDecode((regex.Replace(html, "")));
        }

        public static string EncodeHtml(string ss)
        {
            ss = WebUtility.HtmlEncode(ss);
            var lines = ss.Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.None
                        );
            var htmlLine = lines.Select(c => {
                int sizeC = c.Length;
                c = c.TrimStart();
                sizeC -= c.Length;
                string spaces = string.Concat(Enumerable.Repeat("&nbsp; ", sizeC));
                c = String.Concat(spaces, c);
                return c;
            }).ToList();

            return String.Join("<br />", htmlLine);
        }
        public static ZipArchive ExtractZip(Stream result)
        {
            return new ZipArchive(result, ZipArchiveMode.Create);
        }
    }
}
