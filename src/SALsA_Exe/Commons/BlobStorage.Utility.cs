using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    class BlobStorageUtility
    {

        private static void SendSASToICM(string name, int Id)
        {
            var sasToken = BlobStorage.GetSASToken(Id, name);
            // Since we build our own HTML, we directly call the AddICMDiscussion instead of callign Log.Online
            if (!SALsA.GetInstance(Id).ICM.QueueICMDiscussion(Utility.UrlToHml(name, sasToken), false))
            {
                Log.Information("Did not add ICM discussion : {0} with sasToken {1}. Probably already exists", name, sasToken);
            }
        }

        public static async Task SaveAndSendBlobTask(string name, Task<ZipArchiveEntry> task, int Id)
        {
            name = Utility.FormatFileName(Id, name);
            var output = (await task).Open();
            if (output == null)
            {
                Log.Warning("Could not create File {0} because Task output was null", name);
                return;
            }
            await BlobStorage.UploadStream(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }

        public static async Task SaveAndSendBlobTask(string name, Task<String> task, int Id)
        {
            name = Utility.FormatFileName(Id, name);
            var output = await task;
            await BlobStorage.UploadText(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }
        public static async Task SaveAndSendBlobTask(string name, Task<Image> task, int Id)
        {
            name = Utility.FormatFileName(Id, name);
            var output = await task;
            if (output == null)
            {
                Log.Warning("Could not create File {0} because Task output was null", name);
                return;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                output.Save(ms, ImageFormat.Png);
                await BlobStorage.UploadBytes(Id, name, ms.ToArray(), "image/png");
            }
            SendSASToICM(name, Id);
            BlobStorageUtility.SaveToFile(name, output, Id);
        }

        private static void SaveToFile(string name, Image output, int Id)
        {
            output.Save(Utility.CreateICMFolderInLogDirAndReturnFullPath(name, Id));
        }

        public static async Task SaveAndSendBlobTask(string name, Task<Stream> task, int Id)
        {
            name = Utility.FormatFileName(Id, name);
            var output = await task;
            if (output == null)
            {
                Log.Warning("Could not create File {0} because Task output was null", name);
                return;
            }
            await BlobStorage.UploadStream(Id, name, output);
            SendSASToICM(name, Id);
            Utility.SaveToFile(name, output, Id);
        }
        public static void UploadLog(int Id)
        {
            try
            {
                var currentTime = DateTime.UtcNow.ToString("yyMMddTHHmmss", CultureInfo.InvariantCulture);
                var blobName = String.Format("{0}-{1}_{2}_{3}{4}", Constants.LogFileNamePrefix, Log.UID,
                    currentTime, Id, Constants.LogFileNameExtension);
                Log.FlushAndClose();
                BlobStorage.UploadFile(Id, blobName, Log.LogFullPath, "text/plain").GetAwaiter().GetResult();
                var sas = BlobStorage.GetSASToken(Id, blobName);
                Log.SetSAS(sas);
                Log.Information("Log for this automatic run are available here : {0}", sas);
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to upload log for this run");
                Log.Exception(ex);
            }
        }

        public static string UploadICMRun(int Id, string html)
        {
            try
            {
                var currentTime = DateTime.UtcNow.ToString("yyMMddTHHmmss", CultureInfo.InvariantCulture);
                var blobName = String.Format("{0}-{1}_{2}_{3}{4}", Constants.LogICMFileNamePrefix, Log.UID,
                    currentTime, Id, Constants.LogICMExtension);
                BlobStorage.UploadText(Id, blobName, html, "text/html").GetAwaiter().GetResult();
                BlobStorage.UploadText(Id, Constants.LogICMQuick, html, "text/html").GetAwaiter().GetResult();
                var sas = BlobStorage.GetSASToken(Id, blobName);
                Log.Information("ICM log for this run are available here : {0}", sas);
                return sas;
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to upload ICM log for this run");
                Log.Exception(ex);
                return null;
            }
        }
    }
}
