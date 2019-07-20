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
                if(t != null)
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
                    for(int i = Tasks.Count - 1; i >= 0; --i)
                    {
                        try
                        { 
                            if(!Tasks[i].IsCompleted)
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
        public static string UrlToHml(string name, string url, int size = 30)
        {
            return String.Format("<a href=\"{0}\" style=\"font-size: {2}px;\" >{1}</a>", url, name, size);
        }

        private static void SendSASToICM(string name, int Id)
        {
            var sasToken = BlobStorage.GetSASToken(Id, name);
            // Since we build our own HTML, we directly call the AddICMDiscussion instead of callign SALsA.GetInstance(icm)?.Log.Online
            if (!SALsA.GetInstance(Id).ICM.AddICMDiscussion(Utility.UrlToHml(name, sasToken), false, false))
            {
                SALsA.GetInstance(Id)?.Log.Error("Failed to add to ICM discussion : {0} with sasToken {1}", name, sasToken);
            }
        }

        public static async Task SaveAndSendBlobTask(string name, Task<ZipArchiveEntry> task, int Id)
        {
            var output = (await task).Open();
            await BlobStorage.UploadStream(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }

        public static async Task SaveAndSendBlobTask(string name, Task<String> task, int Id)
        {
            var output = await task; 
            await BlobStorage.UploadText(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Image> task, int Id)
        {
            var output = await task;
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
            var output = await task;
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
    }
}
