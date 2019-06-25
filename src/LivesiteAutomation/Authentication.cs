using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    class Authentication
    {
        public X509Certificate2 cert { get; private set; } = null;
        private Authentication()
        {
            try
            { 
                var serviceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(serviceTokenProvider.KeyVaultTokenCallback));
                SecretBundle secret = keyVaultClient.GetSecretAsync(Constants.AuthenticationCertSecretURI).GetAwaiter().GetResult();
                cert = new X509Certificate2(Convert.FromBase64String(secret.Value), "",
                    X509KeyStorageFlags.DefaultKeySet |
                    X509KeyStorageFlags.MachineKeySet |
                    X509KeyStorageFlags.PersistKeySet |
                    X509KeyStorageFlags.Exportable);
                Log.Instance.Verbose("Successfully got certificate : {0}", cert.SubjectName.Name);
            }
            catch (Exception ex)
            {
                Log.Instance.Critical("Error getting Cerificate from {0} keyvault", Constants.AuthenticationCertSecretURI);
                Log.Instance.Exception(ex);
                throw ex;
            }
        }
        private static Authentication instance = null;
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
    }
}
