using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    static class Constants
    {
        // CRP global
        public readonly static string[] CRPRegions = { "AustraliaCentral", "AustraliaCentral2", "AustraliaEast", "AustraliaSouthEast", "BrazilSouth", "CanadaCentral", "CanadaEast", "CentralIndia", "CentralUS", "CentralUSEUAP", "EastAsia", "EastUS", "EastUS2", "EastUS2EUAP", "FranceCentral", "FranceSouth", "JapanEast", "JapanWest", "KoreaCentral", "KoreaSouth", "NorthCentralUS", "NorthEurope", "SouthAfricaNorth", "SouthAfricaWest", "SouthCentralUS", "SoutheastAsia", "SouthIndia", "UAECentral", "UAENorth", "UKNorth", "UKSouth", "UKSouth2", "UKWest", "WestCentralUS", "WestEurope", "WestIndia", "WestUS", "WestUS2", "WestUSValidation" };

        // Log.cs
        public const string LogFileNamePrefix = "LivesiteAutomation";
        public const string LogFileNameExtension = ".log";

        // Authentication.cs
        public const string AuthenticationCertSecretURI = "https://azvmagent-automation.vault.azure.net/secrets/azurevmguestagentsandextensions-manual/935a9dc4ac9e437d935c0bf26ccfa160";
        public const string AuthenticationServicePrinciaplSecretURI = "https://azvmagent-automation.vault.azure.net/secrets/SP-Geneva-Automation/527a748977ff49bab1437c0723ce0267";
        public const string AuthenticationBlobConnectionStringSecretURI = "https://azvmagent-automation.vault.azure.net/secrets/GenevaAutomationConnectionString/7c7bd5d6f9d144599500a4eb9d9ad4fc";

        // BlobStorage.cs
        public const string BlobStorageVault = "https://genevaautomation.blob.core.windows.net";
        public const string BlobStorageAccount = "incidents";
        public readonly static string BlobStorageConnection = string.Format("{0}/{1}", BlobStorageVault, BlobStorageAccount);

        // ICM.cs
        public const string ICMIdentityName = "CN=azurevmguestagentsandextensions-manual.geneva.keyvault.com";
        public const string ICMBaseUri = "https://icm.ad.msft.net/";
        public const string ICMRelativeBaseAPIUri = "api/cert/incidents";
        public const string ICMTrnasferIncidentSuffix = "/TransferIncident";
        public const string ICMDescriptionEntriesSuffix = "/DescriptionEntries?$inlinecount=allpages";

        // Analyser.cs
        public const string AnalyzerSubscriptionIdField = "SubscriptionId";
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
        public const string AnalyzerInspectIaaSDiskOutputFilename = "InspectIaaSDisk.zip";
        public const string AnalyzerNodeDiagnosticsFilename = "Logs.zip";

        // GetARMSubscriptionRG.cs
        public const string GetARMSubscriptionRGExtensionName = "Azure Resource Manager";
        public const string GetARMSubscriptionRGOperationName = "GetResourceGroupResources";

        // GetRDFESubscription.cs
        public const string GetRDFESubscriptionExtensionName = "AzureRT";
        public const string GetRDFESubscriptionOperationName = "GetSubscriptionWithDetails";
        public const string GetRDFESubscriptionDetailLevel = "Full";

        // GetVMConsoleSerial.cs
        public const string GetVMConsoleSerialLogsExtensionName = "CRP";
        public const string GetVMConsoleSerialLogsOperationName = "GetVMConsoleSerialLogs";
        public const string GetVMSSConsoleSerialLogsOperationName = "GetVMScaleSetVMConsoleSerialLogs";

        // GetVMConsoleScreenshot.cs
        public const string GetVMConsoleScreenshotExtensionName = "CRP";
        public const string GetVMConsoleScreenshotOperationName = "GetVMConsoleScreenshot";
        public const string GetVMSSConsoleScreenshotOperationName = "GetVMScaleSetVMConsoleScreenshot";

        // GetVMModelAndInstanceView.cs
        public const string GetVMInfoExtensionName = "CRP";
        public const string GetVMInfoOperationName = "GetVM";
        public const string GetVMSSInfoOperationName = "GetVMScaleSetVM";
        public readonly static string[] GetVMInfoOptions = { "VM model", "VM InstanceView" };
        public readonly static string[] GetVMInfoOptionsVMSS = { "Model", "InstanceView" };

        // InspectIaaSDiskForARMVM.cs
        public const string InspectIaaSDiskForARMVMExtensionName = "AzLinux";
        public const string InspectIaaSDiskForARMVMOperationName = "InspectIaaSDiskForARMVMEx";
        public const string InspectIaaSDiskForARMVMSSOperationName = "InspectIaaSDiskForARMVmssVMEx";
        public const string InspectIaaSDiskForARMVMMode = "Diagnostic";
        public const string InspectIaaSDiskForARMVMStep = "0";
        public const int InspectIaaSDiskForARMVMTimeout = 0;

        // GetNodeDiagnostics.cs
        public const string GetNodeDiagnosticsExtensionName = "SupportabilityFabric";
        public const string GetNodeDiagnosticsOperatorName = "GetNodeDiagnosticsFileByVMNameAndDeploymentId";
        public const string GetNodeDiagnosticsParam = "GuestAgentVMLogs";

        // GetClassicVMConsoleScreenshot.cs
        public const string GetClassicVMClassicScreenshotExtensionName = "SupportabilityFabric";
        public const string GetClassicVMClassicScreenshotOperatorName = "GetVMScreenshot";
        public const string GetClassicVMClassicScreenshotSize = "XLarge";

        // TODO : Replace this by a Kusto lookup ;)
        /* IncidentHistory | where OwningTeamName  == "NAME" | distinct OwningTenantPublicId | where isnotempty(OwningTenantPublicId) */
        public readonly static Dictionary<string, Guid> ICMTeamToTenantLookupTable = new Dictionary<string, Guid>
                {
                    { @"AZUREVMGUESTAGENTSANDEXTENSIONS\Triage", new Guid("b785142b-3f60-4a7f-a3fe-11ef0941ac1a") }
                };
    }
}
