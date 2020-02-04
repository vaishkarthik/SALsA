﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    static class Constants
    {
        // CRP global
        public readonly static string[] CRPRegions = { "AustraliaCentral", "AustraliaCentral2", "AustraliaEast", "AustraliaSouthEast", "BrazilSouth", "CanadaCentral", "CanadaEast", "CentralIndia", "CentralUS", "CentralUSEUAP", "ChinaEast", "ChinaEast2", "ChinaNorth", "ChinaNorth2", "EastAsia", "EastUS", "EastUS2", "EastUS2EUAP", "FranceCentral", "FranceSouth", "GermanyCentral", "GermanyNorth", "GermanyNortheast", "GermanyWestCentral", "JapanEast", "JapanWest", "KoreaCentral", "KoreaSouth", "NorthCentralUS", "NorthEurope", "NorwayEast", "NorwayWest", "SouthAfricaNorth", "SouthAfricaWest", "SouthcentralUS", "SouthCentralUS", "SoutheastAsia", "SouthEastAsia", "SouthIndia", "Staging (westus2)", "Staging%20(westus2)", "SwitzerlandNorth", "SwitzerlandWest", "UAECentral", "UAENorth", "UKNorth", "UKSouth", "UKSouth2", "UKWest", "USDoDCentral", "USDoDEast", "USGovArizona", "USGovIowa", "USGovTexas", "USGovVirginia", "WestCentralUS", "WestEurope", "WestIndia", "WestUS", "WestUS2", "WestUSValidation" };

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
        public const string AnalyzerHostGAPluginFilename = "HostGAPlugin.zip";
        public const string AnalyzerContainerSettings = "ContainerSettings.json";

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
        public const string InspectIaaSDiskForARMVMOperationName = "InspectIaaSDiskForARMVM";
        public const string InspectIaaSDiskForARMVMSSOperationName = "InspectIaaSDiskForARMVmssVM";
        public const string InspectIaaSDiskForARMVMMode = "Diagnostic";
        public const string InspectIaaSDiskForARMVMStep = "0";
        public const int InspectIaaSDiskForARMVMTimeout = 0;

        // GetNodeDiagnostics.cs
        public const string GetNodeDiagnosticsExtensionName = "SupportabilityFabric";
        public const string GetNodeDiagnosticsOperatorNameDeployment = "GetNodeDiagnosticsFileByVMNameAndDeploymentId";
        public const string GetNodeDiagnosticsOperatorNameFiles = "GetNodeDiagnosticsFiles";
        public const string GetNodeDiagnosticsParam = "GuestAgentVMLogs";
        public const string GetNodeDiagnosticsAllFilesParam = "AllLogs";
        public const string GetNodeDiagnosticsFilesTagsParam = "GuestAgentLogs";

        // GetClassicVMConsoleScreenshot.cs
        public const string GetClassicVMClassicScreenshotExtensionName = "SupportabilityFabric";
        public const string GetClassicVMClassicScreenshotOperatorName = "GetVMScreenshot";
        public const string GetClassicVMClassicScreenshotSize = "XLarge";

        // GetContainerSettings.cs
        public const string GetContainerSettingsExtensionName = "SupportabilityFabric";
        public const string GetContainerSettingsOperatorName = "GetContainerSettings";

        // KustoClient.cs
        public const int KustoClientQueryLimit = 1000;

        // GuestAgentGenericLogs.cs
        public const string KustoGuestAgentGenericLogsCluster = "rdos";
        public const string KustoGuestAgentGenericLogsDataBase = "rdos";
        public const string KustoGuestAgentGenericLogsTable = "GuestAgentGenericLogs";

        // AzureCMVMIdToContainerID.cs
        public const string KustoLogContainerSnapshotCluster = "AzureCM";
        public const string KustoLogContainerSnapshotDatabase = "AzureCM";
        public const string KustoLogContainerSnapshotTable = "LogContainerSnapshot";

        // VMEGAnalysis.cs
        public const string KustoExecutionGraphCluster = "Vmakpi";
        public const string KustoExecutionGraphDatabase = "vmadb";
        public const string KustoExecutionGraphTable = "VmaEgAnalysis";

        // TODO : Replace this by a Kusto lookup ;)
        /* IncidentHistory | where OwningTeamName  == "NAME" | distinct OwningTenantPublicId | where isnotempty(OwningTenantPublicId) */
        public readonly static Dictionary<string, Guid> ICMTeamToTenantLookupTable = new Dictionary<string, Guid>
                {
                    { @"AZUREVMGUESTAGENTSANDEXTENSIONS\Triage", new Guid("b785142b-3f60-4a7f-a3fe-11ef0941ac1a") }
                };
    }
}
