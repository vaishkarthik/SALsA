using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Storage.Auth;
using System;
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
        private static Authentication instance = null;

        public X509Certificate2 Cert
        {
            get
            {
                if (cert == null)
                {
                    Log.Instance.Information("First time calling Cert, creating cert...");
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
                    Log.Instance.Information("First time calling StorageCredentials, creating StorageCredentials...");
                    storageCredentials = PopulateStorageCredentials();
                }
                return storageCredentials;
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
            try
            {
                this.azToken = new AzureServiceTokenProvider();
            }
            catch (Exception ex)
            {
                Log.Instance.Critical("Failed to get token from AzureServiceTokenProvider", Constants.AuthenticationCertSecretURI);
                Log.Instance.Exception(ex);
                throw ex;
            }
        }

        private X509Certificate2 PopulateCertificate()
        {
            try
            {
                // Creating client to connect with keyVault
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azToken.KeyVaultTokenCallback));

                // Getting secret cert in base64 form
                SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationCertSecretURI).GetAwaiter().GetResult();

                // Generating the certificate, with Storage Flag to be compatible with Azure Functions
                var cert = new X509Certificate2(Convert.FromBase64String(secret.Value), "",
                    X509KeyStorageFlags.DefaultKeySet |
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet /*| X509KeyStorageFlags.Exportable*/);
                Log.Instance.Verbose("Successfully got certificate : {0}", cert.SubjectName.Name);

                return cert;
            }
            catch (Exception ex)
            {
                Log.Instance.Critical("Error getting Cerificate from {0} keyvault", Constants.AuthenticationCertSecretURI);
                Log.Instance.Exception(ex);
                throw ex;
            }
        }

        private StorageCredentials PopulateStorageCredentials()
        {
            try
            { 
                var tokenAndFrequency = TokenRenewerAsync(azToken,
                                                          CancellationToken.None).GetAwaiter().GetResult();

                // Create storage credentials using the initial token, and connect the callback function 
                // to renew the token just before it expires
                TokenCredential tokenCredential = new TokenCredential(tokenAndFrequency.Token,
                                                                        TokenRenewerAsync,
                                                                        azToken,
                                                                        tokenAndFrequency.Frequency.Value);

                return new StorageCredentials(tokenCredential);
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Error getting Storage Credentials");
                Log.Instance.Exception(ex);
                return null;
            }
        }

        private static async Task<NewTokenAndFrequency> TokenRenewerAsync(Object state, CancellationToken cancellationToken)
        {
            // Specify the resource ID for requesting Azure AD tokens for Azure Storage.
            const string StorageResource = "https://storage.azure.com/";

            // Use the same token provider to request a new token.
            var authResult = await ((AzureServiceTokenProvider)state).GetAuthenticationResultAsync(StorageResource);

            // Renew the token 5 minutes before it expires.
            var next = (authResult.ExpiresOn - DateTimeOffset.UtcNow) - TimeSpan.FromMinutes(5);
            if (next.Ticks < 0)
            {
                next = default(TimeSpan);
                Log.Instance.Verbose("Renewing Storage token...");
            }

            // Return the new token and the next refresh time.
            return new NewTokenAndFrequency(authResult.AccessToken, next);
        }
    }
}
