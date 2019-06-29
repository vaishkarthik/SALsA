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
        public const string ICMIdentityName = "CN=azurevmguestagentsandextensions-manual.geneva.keyvault.com";
        public const string ICMGetIncidentURL = "https://icm.ad.msft.net/api/cert/incidents";
        public const string ICMTrnasferIncidentSuffix = "/TransferIncident";
        public const string ICMDescriptionEntriesSuffix = "/DescriptionEntries?$inlinecount=allpages";
        public const string ICMAddDiscussionURL = "https://icm.ad.msft.net/api2/incidentapi/incidents";

        // Analyser.cs
        public const string AnalyzerSubscriptionIdField = "subscription";
        public const string AnalyzerStartTimeField = "time";
        public const string AnalyzerVMNameField = "vmname";
        public const string AnalyzerResourceGroupField = "ResourceGroup";
        public const string AnalyzerARMDeploymentExtensionType = "extensions";
        public const string AnalyzerARMDeploymentIaaSType = "virtualMachines";
        public const string AnalyzerARMDeploymentVMSSType = "virtualMachineScaleSets";
        public const string AnalyzerARMDeploymentPaaSType = "domainNames";
        public readonly static string[] AnalyszerARMDeploymentTypes = { AnalyzerARMDeploymentIaaSType, AnalyzerARMDeploymentVMSSType, AnalyzerARMDeploymentPaaSType };
        public const string AnalyzerConsoleSerialOutputFilename = "SerialConsole.log";
        public const string AnalyzerVMScreenshotOutputFilename = "VMScreenshot.png";
        public const string AnalyzerVMModelAndViewOutputFilename = "ModelAndView.json";
        public const string AnalyzerInspectIaaSDiskOutputFilename = "InspectIaaSDisk.zi;p";

        // GetARMSubscription.cs
        public const string GetARMSubscriptionExtensionName = "Azure Resource Manager";
        public const string GetARMSubscriptionOperationName = "GetSubscriptionResources";

        // GetRDFESubscription.cs
        public const string GetRDFESubscriptionExtensionName = "AzureRT";
        public const string GetRDFESubscriptionOperationName = "GetSubscriptionWithDetails";
        public const string GetRDFESubscriptionDetailLevel = "Subscription, HostedService, DeploymentBasic";

        // GetVMScreenshot.cs
        public const string GetVMConsoleSerialLogsExtensionName = "CRP";
        public const string GetVMConsoleSerialLogsOperationName = "GetVMConsoleSerialLogs";

        // GetVMScreenshot.cs
        public const string GetVMScreenshotExtensionName = "CRP";
        public const string GetVMScreenshotOperationName = "GetVMScreenshotLogs";

        // TODO : Replace this by a Kusto lookup ;)
        /* IncidentHistory | where OwningTeamName  == "NAME" | distinct OwningTenantPublicId | where isnotempty(OwningTenantPublicId) */
        public readonly static Dictionary<string, Guid> ICMTeamToTenantLookupTable = new Dictionary<string, Guid>
                {
                    { @"AZUREVMGUESTAGENTSANDEXTENSIONS\Triage", new Guid("b785142b-3f60-4a7f-a3fe-11ef0941ac1a") }
                };
    }
}
