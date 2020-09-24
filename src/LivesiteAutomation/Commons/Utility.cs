using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.HtmlControls;

namespace LivesiteAutomation
{
    public static class Utility
    {
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
                    SALsA.GetInstance(Id)?.Log.Warning("AddTask received a null task");
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
                SALsA.GetInstance(Id)?.Log.Information("Waiting for all {0} tasks...", Tasks.Count);
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
                            SALsA.GetInstance(Id)?.Log.Error("Error waiting for task.");
                            SALsA.GetInstance(Id)?.Log.Exception(ex);
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

        public static string BitMapToHTML(Bitmap bitmap, long quality = 80)
        {
            using (var ms = new MemoryStream())
            {
                EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().FirstOrDefault(o => o.FormatID == ImageFormat.Jpeg.Guid);
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = qualityParam;
                bitmap.Save(ms, imageCodec, parameters);
                var base64 = Convert.ToBase64String(ms.ToArray()); //Get Base64
                return String.Format("<img src=\"data:image/bmp;base64,{0}\" width=\"{1}\" height=\"{2}\" />",
                                            base64, bitmap.Width, bitmap.Height);
            }
        }

        public static string GenerateICMHTMLPage(int Icm, string[] messages)
        {
            StringBuilder sb = new StringBuilder();
            Array.Sort(messages, (x, y) => x.Length.CompareTo(y.Length));
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");

            sb.AppendLine("<h1>");
            sb.AppendLine(String.Format("ICM Log #{0}. {1}", Icm, SALsA.GetInstance(Icm)?.Log.StartTime));
            sb.AppendLine("</h1>");

            sb.AppendLine("<body>");
            foreach (string s in messages)
            {
                sb.AppendLine("<br></br><br></br>");
                sb.AppendLine(s);
            }
            sb.AppendLine("</body>");

            sb.AppendLine("</html>");
            return sb.ToString();
        }

        public static string InitStartTime(int Icm, string dateTime = null)
        {
            if(dateTime != null)
            {
                return dateTime;
            }

            var _startTime = ICM.GetCustomField(Icm, Constants.AnalyzerStartTimeField);
            if (_startTime == null)
            {
                return Kusto.KustoBase<DateTime>.DefaultStartTime;
            }
            else
            {
                return DateTime.Parse(_startTime).AddDays(-1).ToUniversalTime().ToString("o");
            }
        }

        public static DateTime ICMImpactStartTime(int Icm)
        {
            DateTime date;
            DateTime.TryParse(ICM.GetCustomField(Icm, Constants.AnalyzerStartTimeField), out date);
            if (date == null)
            {
                date = SALsA.GetInstance(Icm).ICM.CurrentICM.ImpactStartDate;
            }
            if (date == null)
            {
                date = DateTime.Today.AddDays(-7).ToUniversalTime();
            }
            return date;
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
            string ss = WebUtility.HtmlEncode(obj.ToString());
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

        private static void SendSASToICM(string name, int Id)
        {
            var sasToken = BlobStorage.GetSASToken(Id, name);
            // Since we build our own HTML, we directly call the AddICMDiscussion instead of callign SALsA.GetInstance(icm)?.Log.Online
            if (!SALsA.GetInstance(Id).ICM.AddICMDiscussion(Utility.UrlToHml(name, sasToken), false))
            {
                SALsA.GetInstance(Id)?.Log.Information("Did not add ICM discussion : {0} with sasToken {1}. Probably already exists", name, sasToken);
            }
        }

        private static string FormatFileName(int Id, string name)
        {
            name = String.Format("[{0}]{1}_{2}{3}", Id, Path.GetFileNameWithoutExtension(name),
                DateTime.UtcNow.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture), Path.GetExtension(name));
            return name;
        }

        public static async Task SaveAndSendBlobTask(string name, Task<ZipArchiveEntry> task, int Id)
        {
            name = FormatFileName(Id, name);
            var output = (await task).Open();
            if (output == null)
            {
                SALsA.GetInstance(Id)?.Log.Warning("Could not create File {0} because Task output was null", name);
                return;
            }
            await BlobStorage.UploadStream(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }

        public static async Task SaveAndSendBlobTask(string name, Task<String> task, int Id)
        {
            name = FormatFileName(Id, name);
            var output = await task;
            await BlobStorage.UploadText(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Image> task, int Id)
        {
            name = FormatFileName(Id, name);
            var output = await task;
            if (output == null)
            {
                SALsA.GetInstance(Id)?.Log.Warning("Could not create File {0} because Task output was null", name);
                return;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                output.Save(ms, ImageFormat.Png);
                await BlobStorage.UploadBytes(Id, name, ms.ToArray(), "image/png");
            }
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Stream> task, int Id)
        {
            name = FormatFileName(Id, name);
            var output = await task;
            if (output == null)
            {
                SALsA.GetInstance(Id)?.Log.Warning("Could not create File {0} because Task output was null", name);
                return;
            }
            await BlobStorage.UploadStream(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }
        private static string CreateICMFolderInLogDirAndReturnFullPath(string name, int Id)
        {
            var logDir = Path.Combine(Path.GetDirectoryName(SALsA.GetInstance(Id)?.Log.LogFullPath), Convert.ToString(Id));
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            return Path.Combine(logDir, name);
        }
        private static void SaveToFile(string name, string output, int Id)
        {
            File.WriteAllText(CreateICMFolderInLogDirAndReturnFullPath(name, Id), output);
        }
        private static void SaveToFile(string name, Image output, int Id)
        {
            output.Save(CreateICMFolderInLogDirAndReturnFullPath(name, Id));
        }
        private static void SaveToFile(string name, Stream output, int Id)
        {
            SaveToFile(name, StreamToBytes(output), Id);
        }
        private static void SaveToFile(string name, byte[] output, int Id)
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

        public static void UploadLog(int Id)
        {
            try
            {
                var currentTime = DateTime.UtcNow.ToString("yyMMddTHHmmss", CultureInfo.InvariantCulture);
                var blobName = String.Format("{0}-{1}_{2}_{3}{4}", Constants.LogFileNamePrefix, SALsA.GetInstance(Id)?.Log.UID,
                    currentTime, Id, Constants.LogFileNameExtension);
                SALsA.GetInstance(Id)?.Log.FlushAndClose();
                BlobStorage.UploadFile(Id, blobName, SALsA.GetInstance(Id)?.Log.LogFullPath, "text/plain").GetAwaiter().GetResult();
                var sas = BlobStorage.GetSASToken(Id, blobName);
                SALsA.GetInstance(Id)?.Log.Information("Log for this automatic run are available here : {0}", sas);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Warning("Failed to upload log for this run");
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
        }

        public static string UploadICMRun(int Id, string html)
        {
            try
            {
                var currentTime = DateTime.UtcNow.ToString("yyMMddTHHmmss", CultureInfo.InvariantCulture);
                var blobName = String.Format("{0}-{1}_{2}_{3}{4}", Constants.LogICMFileNamePrefix, SALsA.GetInstance(Id)?.Log.UID,
                    currentTime, Id, Constants.LogICMExtension);
                SALsA.GetInstance(Id)?.Log.FlushAndClose();
                BlobStorage.UploadText(Id, blobName, html, "text/html").GetAwaiter().GetResult();
                BlobStorage.UploadText(Id, Constants.LogICMQuick, html, "text/html").GetAwaiter().GetResult();
                var sas = BlobStorage.GetSASToken(Id, blobName);
                SALsA.GetInstance(Id)?.Log.Information("ICM log for this run are available here : {0}", sas);
                return sas;
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Warning("Failed to upload ICM log for this run");
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return null;
            }
        }

        public static string List2DToHTML<T>(List<T[]> result, bool raw = false, bool fromKusto = false)
        {
            if(fromKusto)
            {
                return (result.Count > Constants.MaxResultCount ? "<details><summary>Results (click here for details)</summary>" : "") +
                       "<table style=\"margin-right:auto;margin-left:auto;width:95%;overflow-x:scroll;overflow-y:scroll;height:500px;display:block;\">" +
                       List2DToHTMLInternal(result, raw) +
                       "</table>" + (result.Count > Constants.MaxResultCount ? "</details>" : "");
            }
            else
            {
                return "<table style=\"margin-right:auto;margin-left:auto;width:25%;\">" +
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
                    if (i == 0)
                    {
                        sw.WriteLine("<tr style=\"text-align:left;padding-top:0.5em;padding-bottom:0.5em;font-weight:bold;font-size:22;\" bgcolor=\"#d3d3d3\">");
                    }
                    else
                    {
                        sw.WriteLine("<tr style=\"text-align:left;padding-top:0.5em;padding-bottom:0.5em;\">");
                    }
                    for (int j = 0; j < result[i].Length; ++j)
                    {
                        sw.Write("<td>");
                        if (raw == true)
                        {
                            sw.Write(result[i][j]);
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
