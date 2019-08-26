using Bond.Tag;
using LivesiteAutomation.Json2Class;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Microsoft.WindowsAzure.Security.CredentialsManagement.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class Authentication
    {
        private X509Certificate2 cert = null;
        private StorageCredentials storageCredentials = null;
        private AzureServiceTokenProvider azToken = null;
        private KeyVaultClient keyVaultClient = null;
        private ServicePrincipal servicePrincipal = null;
        private static Authentication instance = null;

        public X509Certificate2 Cert
        {
            get
            {
                if (cert == null)
                {
                    SALsA.GlobalLog?.Information("First time calling Cert, creating cert...");
                    cert = PopulateCertificate();
                }
                return cert;
            }
        }
        public StorageCredentials StorageCredentials
        {
            get
            {
                if (storageCredentials == null)
                {
                    SALsA.GlobalLog?.Information("First time calling StorageCredentials, creating StorageCredentials...");
                    storageCredentials = PopulateStorageCredentials();
                }
                return storageCredentials;
            }
        }

        public ServicePrincipal ServicePrincipal
        {
            get
            {
                if (servicePrincipal == null)
                {
                    SALsA.GlobalLog?.Information("First time calling servicePrincipal, creating servicePrincipal...");
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
                SALsA.GlobalLog?.Critical("Failed to get token from AzureServiceTokenProvider", Constants.AuthenticationCertSecretURI);
                SALsA.GlobalLog?.Exception(ex);
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
                SALsA.GlobalLog?.Verbose("Successfully got certificate : {0}", cert.SubjectName.Name);

                return cert;
            }
            catch (Exception ex)
            {
                SALsA.GlobalLog?.Critical("Error getting Cerificate from {0} keyvault", Constants.AuthenticationCertSecretURI);
                SALsA.GlobalLog?.Exception(ex);
                throw ex;
            }
        }

        private ServicePrincipal PopulateServicePrincipal()
        {

            SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationServicePrinciaplSecretURI).GetAwaiter().GetResult();
            return Utility.JsonToObject<ServicePrincipal>(secret.Value);

        }

        private StorageCredentials PopulateStorageCredentials()
        {
            try
            {
                SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationBlobConnectionStringSecretURI).GetAwaiter().GetResult();
                return GetStorageCredentials(ParseStringIntoSettings(secret.Value));
            }
            catch (Exception ex)
            {
                SALsA.GlobalLog?.Error("Error getting Storage Credentials");
                SALsA.GlobalLog?.Exception(ex);
                return null;
            }
        }

        // Inspired from : https://msazure.visualstudio.com/One/_git/AAPT-Antares-Websites?path=%2Fsrc%2FHosting%2FAzure%2Ftools%2Fsrc%2FSasHelper%2FSasHelper.cs&version=GBdev
        private static StorageCredentials GetStorageCredentials(IDictionary<string, string> settings)
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
                return new StorageCredentials(accountName, keyValue, keyName);
            if (accountName == null && keyValue == null && (keyName == null && sasToken != null))
                return new StorageCredentials(sasToken);
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
