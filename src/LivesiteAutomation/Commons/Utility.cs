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

            private List<Task> tasks = null;

            private static TaskManager instance = null;
            public static TaskManager Instance
            {
                get
                {
                    if (instance == null)
                    {
                        Log.Instance.Information("Creating task bag..");
                        instance = new TaskManager();
                    }
                    return Instance;
                }
            }

            private TaskManager()
            {
                tasks = new List<Task>();
            }

            public void AddTask(Task t)
            {
                tasks.Add(t);
            }

            public void WaitAllTasks()
            {
                try
                {
                    Task.WaitAll(tasks.ToArray());
                }
                catch
                {
                    for(int i = tasks.Count - 1; i >= 0; --i)
                    {
                        try
                        { 
                            if(!tasks[i].IsCompleted)
                            {
                                tasks[i].GetAwaiter().GetResult();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Instance.Error("Error waiting for task.");
                            Log.Instance.Exception(ex);
                        }
                        finally
                        {
                            tasks.RemoveAt(i);
                        }
                    }
                }
                finally
                {
                    // Should be empty, but just making sure...
                    tasks = new List<Task>();
                }
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
        private static string UrlToHml(string name, string sasToken, int size = 30)
        {
            return String.Format("<a href=\"{0}\" style=\"font-size: {2}px;\" >{1}</a>", sasToken, name, size);
        }

        private static void SendSASToICM(string name)
        {
            var sasToken = BlobStorage.GetSASToken(ICM.Instance.Id, name);
            // Since we build our own HTML, we directly call the AddICMDiscussion instead of callign Log.Instance.Online
            if (ICM.Instance.AddICMDiscussion(Utility.UrlToHml(name, sasToken), false, false))
            {
                Log.Instance.Error("Failed to add to ICM discussion : {0} with sasToken {1}", name, sasToken);
            }
        }

        public static async Task SaveAndSendBlobTask(string name, Task<ZipArchiveEntry> task)
        {
            var output = (await task).Open();
            await BlobStorage.UploadStream(ICM.Instance.Id, name, output);
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }

        public static async Task SaveAndSendBlobTask(string name, Task<String> task)
        {
            var output = await task; 
            await BlobStorage.UploadText(ICM.Instance.Id, name, output);
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Image> task)
        {
            var output = await task;
            using (MemoryStream ms = new MemoryStream())
            {
                output.Save(ms, ImageFormat.Png);
                await BlobStorage.UploadBytes(ICM.Instance.Id, name, ms.ToArray(), "image/png");
            }
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Stream> task)
        {
            var output = await task;
            await BlobStorage.UploadStream(ICM.Instance.Id, name, output);
            SendSASToICM(name);
            Utility.SaveToFile(name, output);
        }
        private static string CreateICMFolderInLogDirAndReturnFullPath(string name)
        {
            var logDir = Path.Combine(Path.GetDirectoryName(Constants.LogDefaultPath), Convert.ToString(ICM.Instance.Id));
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
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
            SaveToFile(name, StreamToBytes(output));
        }
        private static void SaveToFile(string name, byte[] output)
        {
            using (var fileStream = File.Create(CreateICMFolderInLogDirAndReturnFullPath(name)))
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

        public static void UploadLog()
        {
            var currentTime = DateTime.UtcNow.ToString("yyMMddTHHmmss", CultureInfo.InvariantCulture);
            BlobStorage.UploadFile(ICM.Instance.Id, String.Format("{0}_{1}-[{2}].log", currentTime, ICM.Instance.Id, Log.Instance.UID), Constants.LogDefaultPath, "text/plain");
        }
    }
}
