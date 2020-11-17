using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.IdentityModel.Protocols.WSIdentity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public class BlobStorage
    {
        private BlobStorage()
        {
        }

        private static CloudBlockBlob GetCloudBlob(int icm, string blobName)
        {
            return new CloudBlockBlob(new Uri(String.Format("{0}/{1}/{2}", Constants.BlobStorageConnection, icm, blobName)),
                                                            Authentication.Instance.BlobStorageCredentials);
        }

        // TODO : Refactor to use Templates <T> instead
        public static Task UploadText(int icm, string blobName, string content, string contentType = "text/plain")
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                blob.Properties.ContentType = contentType;
                return blob.UploadTextAsync(content);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to write to blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }
        public static Task UploadStream(int icm, string blobName, Stream content, string contentType = "application/octet-stream")
        {
            return UploadBytes(icm, blobName, Utility.StreamToBytes(content), contentType);
        }
        public static Task UploadBytes(int icm, string blobName, byte[] content, string contentType = "application/octet-stream")
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                blob.Properties.ContentType = contentType;
                return blob.UploadFromByteArrayAsync(content, 0, content.Length);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to write to blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }
        public static Task UploadFile(int icm, string blobName, string path, string contentType = "application/octet-stream")
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                blob.Properties.ContentType = contentType;
                return blob.UploadFromFileAsync(path);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read from blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }

        public static string DownloadBlobAsText(int icm, string blobName)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.DownloadText();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read from blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }
        public static Task DownloadBlobToFile(int icm, string blobName, string path)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.DownloadToFileAsync(path, FileMode.Create);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read from blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }
        public static Task DownloadBlobToStream(int icm, string blobName, Stream stream)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.DownloadToStreamAsync(stream);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to read from blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }

        public static string GetSASToken(int icm, string blobName, int ExpiryInHours = Constants.BlobTimeoutInHours, Task blobTask = null)
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
                Log.Information("Generated a valid SAS key for {0} : {1}{2}", blobName, blob.StorageUri.PrimaryUri.AbsoluteUri, sas);
                return String.Format("{0}{1}", blob.StorageUri.PrimaryUri.AbsoluteUri, sas);
            }
            catch (Exception ex)
            {
                Log.Error("Failed to generate SAS from blob : {0}", blobName);
                Log.Exception(ex);
                return null;
            }
        }

        public static bool BlobExist(int icm, string blobName)
        {
            try
            {
                CloudBlockBlob blob = GetCloudBlob(icm, blobName);
                return blob.Exists();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to check storage for blob : {0}", blobName);
                Log.Exception(ex);
                return false;
            }
        }

    }
}
