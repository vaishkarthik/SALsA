using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

        public static T JsonToObject<T>(dynamic dyn)
        {
            var json = ObjectToJson(dyn);
            return JsonToObject<T>(json);
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

        public static string Beautify(dynamic obj)
        {
            return ObjectToJson(obj, true);
        }

        public static ZipArchive ExtractZip(Stream result)
        {
            return new ZipArchive(result, ZipArchiveMode.Read);
        }
        private static string UrlToHml(string name, string sasToken, int size = 30)
        {
            return String.Format("<a style=\"font-size: {2}px;\" href=\"{0}\">{1}</a>", name, sasToken, size);
        }

        private static void SendSASToICM(string name)
        {
            var sasToken = BlobStorage.GetSASToken(Log.Instance.Icm, name);
            // Since we build our own HTML, we directly call the AddICMDiscussion instead of callign Log.Instance.Online
            if (!ICM.IncidentMapping[Log.Instance.Icm].AddICMDiscussion(Utility.UrlToHml(name, sasToken), false))
            {
                Log.Instance.Error("Failed to add to ICM discussion :  {0} with sasToken {1}", name, sasToken);
            }
        }
        public static async Task SaveAndSendBlobTask(string name, Task<String> task)
        {
            var output = await task; 
            await BlobStorage.UploadText(Log.Instance.Icm, name, output);
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Image> task)
        {
            var output = await task;
            using (MemoryStream ms = new MemoryStream())
            {
                output.Save(ms, ImageFormat.Png);
                await BlobStorage.UploadStream(Log.Instance.Icm, name, ms);
            }
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Stream> task)
        {
            var output = await task;
            await BlobStorage.UploadStream(Log.Instance.Icm, name, output);
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }
        private static string CreateICMFolderInLogDirAndReturnFullPath(string name)
        {
            var logDir = Path.Combine(Constants.LogDefaultPath, Log.Instance.Icm);
            if (!File.Exists(Constants.LogDefaultPath))
            {
                new System.IO.FileInfo(Constants.LogDefaultPath).Directory.Create();
            }
            return Path.Combine(logDir, name);
        }
        private static void SaveToFile(string name, string output)
        {
            File.WriteAllText(CreateICMFolderInLogDirAndReturnFullPath(name), output);
        }
        private static void SaveToFile(string name, Image output)
        {
            output.Save(CreateICMFolderInLogDirAndReturnFullPath(name));
        }
        private static void SaveToFile(string name, Stream output)
        {
            using (var fileStream = File.Create(CreateICMFolderInLogDirAndReturnFullPath(name)))
            {
                output.Seek(0, SeekOrigin.Begin);
                output.CopyTo(fileStream);
            }
        }
    }
}
