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

        // ICM.cs
        public const string ICMGetIncidentURL = "https://icm.ad.msft.net/api/cert/incidents";
    }
}
