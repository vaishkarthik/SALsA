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

namespace SALsA.General
{
    public enum SALsAState
    {
        Running, // SALsA is still processing this one !
        Done, // SALsA finished running
        NotFound, // VM not found
        Ignore, // Skipping, probably due to uncompatibility (ex: HGAP dont provide usefull information)
        UnknownException, // Error ! @aydjella needs to take a look
        MissingInfo, // No subid or vmname given
        Queued, // Not yet received by SALsA, but QUeued by the azure function
        ICMNotAccessible // ICM id outside of AzureRT, not accessible
    }

    public static class Utility
    {
        public static Log GlobalLog = new Log();
        public class TaskManager
        {

            private List<Task> Tasks = null;
            private int Id;

            public TaskManager(int icm)
            {
                Tasks = new List<Task>();
                Id = icm;
            }

            public void AddOneTask(Task t)
            {
                if (t != null)
                {
                    Tasks.Add(t);
                }
                else
                {
                    GlobalLog.Warning("AddTask received a null task");
                }
            }

            public void AddTask(params Task[] tasks)
            {
                foreach (var t in tasks)
                {
                    AddOneTask(t);
                }
            }

            public void WaitAllTasks()
            {
                GlobalLog.Information("Waiting for all {0} tasks...", Tasks.Count);
                try
                {
                    Task.WaitAll(Tasks.ToArray());
                }
                catch
                {
                    for (int i = Tasks.Count - 1; i >= 0; --i)
                    {
                        try
                        {
                            if (!Tasks[i].IsCompleted)
                            {
                                Tasks[i].GetAwaiter().GetResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            GlobalLog.Error("Error waiting for task.");
                            GlobalLog.Exception(ex);
                        }
                        finally
                        {
                            Tasks.RemoveAt(i);
                        }
                    }
                }
                finally
                {
                    // Should be empty, but just making sure...
                    Tasks = new List<Task>();
                }
            }
        }

        public static string GenerateICMHTMLPage(int Icm, string[] messages, string startTime)
        {
            StringBuilder sb = new StringBuilder();
            if(messages.Count() > 1)
            {
                Array.Sort(messages, (x, y) => x.Length.CompareTo(y.Length));
            }    
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");

            sb.AppendLine("<h1>");
            sb.AppendLine(String.Format("ICM Log #{0}. {1}", Icm, startTime));
            sb.AppendLine("</h1>");

            sb.AppendLine("<body>");
            foreach (string s in messages)
            {
                sb.AppendLine("<br><br>");
                sb.AppendLine(s);
            }
            sb.AppendLine("</body>");

            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public static string ShortRandom
        {
            get
            {
                return Guid.NewGuid().ToString("n").Substring(0, 8);
            }
        }

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

        public static string EncodeHtml(object obj)
        {
            string ss = WebUtility.HtmlEncode(obj == null ? "N/A" : obj.ToString());
            var lines = ss.Split(
                            new[] { Environment.NewLine },
                            StringSplitOptions.None
                        );
            var htmlLine = lines.Select(c =>
            {
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
        public static string UrlToHml(string name, string url, int size = 30)
        {
            return String.Format("<a href=\"{0}\" style=\"font-size: {2}px;\" >{1}</a>", url, name, size);
        }

        internal static string FormatFileName(int Id, string name)
        {
            name = String.Format("[{0}]{1}_{2}{3}", Id, Path.GetFileNameWithoutExtension(name),
                DateTime.UtcNow.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture), Path.GetExtension(name));
            return name;
        }

        internal static string CreateICMFolderInLogDirAndReturnFullPath(string name, int Id)
        {
            var logDir = Path.Combine(Path.GetDirectoryName(GlobalLog.LogFullPath), Convert.ToString(Id));
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            return Path.Combine(logDir, name);
        }
        internal static void SaveToFile(string name, string output, int Id)
        {
            File.WriteAllText(CreateICMFolderInLogDirAndReturnFullPath(name, Id), output);
        }
        internal static void SaveToFile(string name, Stream output, int Id)
        {
            SaveToFile(name, StreamToBytes(output), Id);
        }
        internal static void SaveToFile(string name, byte[] output, int Id)
        {
            using (var fileStream = File.Create(CreateICMFolderInLogDirAndReturnFullPath(name, Id)))
            {
                fileStream.Write(output, 0, output.Length);
            }
        }
        public static byte[] StreamToBytes(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static string List2DToHTML<T>(List<T[]> result, bool raw = false, bool fromKusto = false)
        {
            if(fromKusto)
            {
                return (result.Count > Constants.MaxResultCount ? "<details><summary>Results (click here for details)</summary>" : "") +
                       "<table style=\"margin-right:auto;margin-left:auto;width:80%;overflow-x:scroll;overflow-y:scroll;height:500px;display:block;\">" +
                       List2DToHTMLInternal(result, raw) +
                       "</table>" + (result.Count > Constants.MaxResultCount ? "</details>" : "");
            }
            else
            {
                return "<table style=\"margin-right:auto;margin-left:auto;width:90%;border-collapse:separate;border-spacing:0 1em;border-style: hidden;\">" +
                       List2DToHTMLInternal(result, raw) +
                       "</table>";
            }
        }


        private static string List2DToHTMLInternal<T>(List<T[]> result, bool raw = false)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine("<tbody>");
                for (int i = 0; i < result.Count; ++i)
                {
                    sw.WriteLine(String.Format("<tr style=\"text-align:left;font-size:20;{0}\" {1}>",
                        i == 0 ? "font-weight:bold;" : "", i == 0 ? "bgcolor=\"#d3d3d3\"" : ""));
                    for (int j = 0; j < result[i].Length; ++j)
                    {
                        sw.Write("<td style=\"padding-right: 10px;padding-left: 10px\">");
                        if (raw == true)
                        {
                            if(result[i][j] == null)
                            {
                                sw.Write("N/A");
                            }
                            else {
                                sw.Write(result[i][j]);
                            }
                        }
                        else
                        {
                            sw.Write(Utility.EncodeHtml(result[i][j]));
                        }
                        sw.WriteLine("</td>");
                    }
                    sw.WriteLine("</tr>");
                }
                sw.WriteLine("</tbody>");
                return sw.ToString();
            }
        }

        public static byte[] CompressString(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (var outputStream = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    gZipStream.Write(inputBytes, 0, inputBytes.Length);

                return outputStream.ToArray();
            }
        }

        public static string DecompressString(byte[] input)
        {
            using (var inputStream = new MemoryStream(input))
            using (var gZipStream = new GZipStream(inputStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gZipStream))
            {
                return streamReader.ReadToEnd();
            }
        }

        public static string Base64Encode(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            return Base64Encode(inputBytes);
        }

        public static string Base64Encode(byte[] input)
        {
            return Convert.ToBase64String(input);
        }
        public static string Base64Decode(string input)
        {
            return ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(input));
        }
    }
}
