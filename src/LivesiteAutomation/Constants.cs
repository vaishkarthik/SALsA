using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    static class Constants
    {
        // Log.cs
        public static string LogDefaultPath = System.IO.Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Log", LogFileName);
        public const string LogFileName = "LivesiteAutomation.log";

        // Authentication.cs
        public const string AuthenticationCertSecretURI = "https://azvmagent-automation.vault.azure.net/secrets/azurevmguestagentsandextensions-manual/935a9dc4ac9e437d935c0bf26ccfa160";

        // BlobStorage.cs
        public const string BlobStorageConnection = "https://genevaautomation.blob.core.windows.net/incidents/";

        // ICM.cs
        public const string ICMGetIncidentURL = "https://icm.ad.msft.net/api/cert/incidents";
        public const string ICMTrnasferIncidentSuffix = "/TransferIncident";

        // TODO : Replace this by a Kusto lookup ;)
        /* IncidentHistory | where OwningTeamName  == "NAME" | distinct OwningTenantPublicId | where isnotempty(OwningTenantPublicId) */
        public static Dictionary<string, Guid> ICMTeamToTenantLookupTable = new Dictionary<string, Guid>
        {
            { @"AZUREVMGUESTAGENTSANDEXTENSIONS\Triage", new Guid("b785142b-3f60-4a7f-a3fe-11ef0941ac1a") }
        };
    }
}
