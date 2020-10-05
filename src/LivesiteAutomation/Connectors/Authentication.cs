using SALsA.LivesiteAutomation.Json2Class;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace SALsA.General
{
    class Authentication
    {
        private X509Certificate2 cert = null;
        private Microsoft.Azure.Storage.Auth.StorageCredentials blobStorageCredentials = null;
        private Microsoft.Azure.Cosmos.Table.CloudTable tableStorageClient = null;
        private AzureServiceTokenProvider azToken = null;
        private KeyVaultClient keyVaultClient = null;
        private ServicePrincipal servicePrincipal = null;
        private static Authentication instance = null;
        public static Log GlobalLog = null;

        public X509Certificate2 Cert
        {
            get
            {
                if (cert == null)
                {
                    GlobalLog?.Information("First time calling Cert, creating cert...");
                    cert = PopulateCertificate();
                }
                return cert;
            }
        }

        public Microsoft.Azure.Storage.Auth.StorageCredentials BlobStorageCredentials
        {
            get
            {
                if (blobStorageCredentials == null)
                {
                    GlobalLog?.Information("First time calling BlobStorageCredentials, creating StorageCredentials...");
                    blobStorageCredentials = PopulateBlobStorageCredentials();
                }
                return blobStorageCredentials;
            }
        }
        public Microsoft.Azure.Cosmos.Table.CloudTable TableStorageClient
        {
            get
            {
                if (tableStorageClient == null)
                {
                    GlobalLog?.Information("First time calling TableStorageCredentials, creating StorageCredentials...");
                    var creds = PopulateTableStorageCredentials();
                    var tableStorageAccount = new CloudStorageAccount(creds, new Uri(Constants.TableStorageVault));
                    var client = tableStorageAccount.CreateCloudTableClient();
                    tableStorageClient = client.GetTableReference(Constants.TableStorageAccount);
                }
                return tableStorageClient;
            }
        }

        public ServicePrincipal ServicePrincipal
        {
            get
            {
                if (servicePrincipal == null)
                {
                    GlobalLog?.Information("First time calling servicePrincipal, creating servicePrincipal...");
                    servicePrincipal = PopulateServicePrincipal();
                }
                return servicePrincipal;
            }
        }

        public static Authentication Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Authentication();
                }
                return instance;
            }
        }

        private Authentication()
        {
            // TODO : make this awaitable or async
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                this.azToken = new AzureServiceTokenProvider();
                this.keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azToken.KeyVaultTokenCallback));
            }
            catch (Exception ex)
            {
                GlobalLog?.Critical("Failed to get token from AzureServiceTokenProvider", Constants.AuthenticationCertSecretURI);
                GlobalLog?.Exception(ex);
                throw ex;
            }
        }

        private X509Certificate2 PopulateCertificate()
        {
            try
            {
                // Getting secret cert in base64 form
                SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationCertSecretURI).GetAwaiter().GetResult();

                // Generating the certificate, with Storage Flag to be compatible with Azure Functions
                var cert = new X509Certificate2(Convert.FromBase64String(secret.Value), "",
                    X509KeyStorageFlags.DefaultKeySet |
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet /*| X509KeyStorageFlags.Exportable*/);
                GlobalLog?.Verbose("Successfully got certificate : {0}", cert.SubjectName.Name);

                return cert;
            }
            catch (Exception ex)
            {
                GlobalLog?.Critical("Error getting Cerificate from {0} keyvault", Constants.AuthenticationCertSecretURI);
                GlobalLog?.Exception(ex);
                throw ex;
            }
        }

        private ServicePrincipal PopulateServicePrincipal()
        {

            SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationServicePrincioalSecretURI).GetAwaiter().GetResult();
            return Utility.JsonToObject<ServicePrincipal>(secret.Value);

        }

        private Microsoft.Azure.Storage.Auth.StorageCredentials PopulateBlobStorageCredentials()
        {
            try
            {
                SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationBlobConnectionStringSecretURI).GetAwaiter().GetResult();
                return GetBlobStorageCredentials(ParseStringIntoSettings(secret.Value));
            }
            catch (Exception ex)
            {
                GlobalLog?.Error("Error getting Storage Credentials");
                GlobalLog?.Exception(ex);
                return null;
            }
        }

        private Microsoft.Azure.Cosmos.Table.StorageCredentials PopulateTableStorageCredentials()
        {
            try
            {
                SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationBlobConnectionStringSecretURI).GetAwaiter().GetResult();
                return GetTableStorageCredentials(ParseStringIntoSettings(secret.Value));
            }
            catch (Exception ex)
            {
                GlobalLog?.Error("Error getting Storage Credentials");
                GlobalLog?.Exception(ex);
                return null;
            }
        }

        // Inspired from : https://msazure.visualstudio.com/One/_git/AAPT-Antares-Websites?path=%2Fsrc%2FHosting%2FAzure%2Ftools%2Fsrc%2FSasHelper%2FSasHelper.cs&version=GBdev
        private static Microsoft.Azure.Storage.Auth.StorageCredentials GetBlobStorageCredentials(IDictionary<string, string> settings)
        {
            string accountName;
            settings.TryGetValue("AccountName", out accountName);
            string keyValue;
            settings.TryGetValue("AccountKey", out keyValue);
            string keyName;
            settings.TryGetValue("AccountKeyName", out keyName);
            string sasToken;
            settings.TryGetValue("SharedAccessSignature", out sasToken);
            if (accountName != null && keyValue != null && sasToken == null)
                return new Microsoft.Azure.Storage.Auth.StorageCredentials(accountName, keyValue, keyName);
            if (accountName == null && keyValue == null && (keyName == null && sasToken != null))
                return new Microsoft.Azure.Storage.Auth.StorageCredentials(sasToken);
            return null;
        }
        private static Microsoft.Azure.Cosmos.Table.StorageCredentials GetTableStorageCredentials(IDictionary<string, string> settings)
        {
            string accountName;
            settings.TryGetValue("AccountName", out accountName);
            string keyValue;
            settings.TryGetValue("AccountKey", out keyValue);
            string keyName;
            settings.TryGetValue("AccountKeyName", out keyName);
            string sasToken;
            settings.TryGetValue("SharedAccessSignature", out sasToken);
            if (accountName != null && keyValue != null && sasToken == null)
                return new Microsoft.Azure.Cosmos.Table.StorageCredentials(accountName, keyValue, keyName);
            if (accountName == null && keyValue == null && (keyName == null && sasToken != null))
                return new Microsoft.Azure.Cosmos.Table.StorageCredentials(sasToken);
            return null;
        }

        private static IDictionary<string, string> ParseStringIntoSettings(string connectionString)
        {
            IDictionary<string, string> dictionary = new Dictionary<string, string>();
            string str1 = connectionString;
            char[] separator1 = new char[1] { ';' };
            int num = 1;
            foreach (string str2 in str1.Split(separator1, (StringSplitOptions)num))
            {
                char[] separator2 = new char[1] { '=' };
                int count = 2;
                string[] strArray = str2.Split(separator2, count);
                if (strArray.Length != 2)
                {
                    return null;
                }
                if (dictionary.ContainsKey(strArray[0]))
                {
                    return null;
                }
                dictionary.Add(strArray[0], strArray[1]);
            }
            return dictionary;
        }
    }
}
