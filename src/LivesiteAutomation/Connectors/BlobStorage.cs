using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public class BlobStorage
    {
        private BlobStorage()
        {
        }

        private static CloudBlockBlob GetCloudBlob(string icm, string blobName)
        {
            return new CloudBlockBlob(new Uri(Constants.BlobStorageConnection + "/" + icm + "/" + blobName),
                                                            Authentication.Instance.StorageCredentials);
        }

        // TODO : Refactor to use Templates <T> instead
        public static Task UploadText(string icm, string blobName, string content, string contentType = "text/plain")
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                blob.Properties.ContentType = contentType;
                return blob.UploadTextAsync(content);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to write to blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }
        public static Task UploadStream(string icm, string blobName, Stream content, string contentType = "application/octet-stream")
        {
            return UploadBytes(icm, blobName, Utility.StreamToBytes(content), contentType);
        }
        public static Task UploadBytes(string icm, string blobName, byte[] content, string contentType = "application/octet-stream")
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                blob.Properties.ContentType = contentType;
                return blob.UploadFromByteArrayAsync(content, 0, content.Length);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to write to blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }
        public static Task UploadFile(string icm, string blobName, string path, string contentType = "application/octet-stream")
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                blob.Properties.ContentType = contentType;
                return blob.UploadFromFileAsync(path);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to read from blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }

        public static string DownloadBlobAsText(string icm, string blobName)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.DownloadText();
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to read from blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }
        public static Task DownloadBlobToFile(string icm, string blobName, string path)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.DownloadToFileAsync(path, FileMode.Create);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to read from blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }
        public static Task DownloadBlobToStream(string icm, string blobName, Stream stream)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.DownloadToStreamAsync(stream);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to read from blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }

        public static string GetSASToken(string icm, string blobName, int ExpiryInHours = 7 * 24, Task blobTask = null)
        {
            try
            {
                if (blobTask != null)
                {
                    // If we are provided with an ongoing task, lets wait for it to end before generating the SAS token;
                    blobTask.Wait();
                }
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                var sas = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy()
                {
                    SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(ExpiryInHours),
                    Permissions = SharedAccessBlobPermissions.Read
                });
                return String.Format("{0}{1}", blob.StorageUri.PrimaryUri.AbsoluteUri, sas);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to generate SAS from blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return null;
            }
        }

        public static bool BlobExist(string icm, string blobName)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.Exists();
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to check storage for blob : {0}", blobName);
                Log.Instance.Exception(ex);
                return false;
            }
        }

    }
}
