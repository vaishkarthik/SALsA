using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public class Incident
    {
        public class NewDiscussionEntry
        {
            public string Description { get; set; }
            public List<object> CustomFields { get; set; }
            public string Id { get; set; }
        }
        public class Transfer
        {
            public class TransferParameters
            {
                public class Description
                {
                    public string Text = "AzureRT Automatically transfering this incident to the rightful owners.";
                    public string RenderType = "Plaintext";
                    public string ChangeBy = "azGaExt";
                }

                public string OwningTenantPublicId = null;
                public string OwningTeamPublicId = null;
                public Description description = new Description();
            }
            public TransferParameters transferParameters = new TransferParameters();
            public Transfer(string owningTeamPublicId)
            {
                transferParameters.OwningTeamPublicId = owningTeamPublicId;
                // TODO replace by Kusto function
                transferParameters.OwningTenantPublicId = Constants.ICMTeamToTenantLookupTable[transferParameters.OwningTeamPublicId].ToString();
            }
        }
        public class DescriptionEntry
        {
            public string DescriptionEntryId;
            public string SubmittedBy;
            public string Cause;
            public DateTime SubmitDate;
            public string Text;
        }
        public class DescriptionPost
        {
            public class NewDescriptionEntryClass
            { 
                public string Text;
                public string RenderType;
            }

            public NewDescriptionEntryClass NewDescriptionEntry;
            public DescriptionPost(string entry, string renderType = "Html")
            {
                NewDescriptionEntry = new NewDescriptionEntryClass();
                NewDescriptionEntry.Text = entry;
                NewDescriptionEntry.RenderType = renderType;
            }
        }

        public class IncidentAcknowledgementData
        {
            public string AcknowledgeContactAlias { get; set; }
            public DateTime AcknowledgeDate { get; set; }
            public bool IsAcknowledged { get; set; }
            public object NotificationId { get; set; }
        }

        public class IncidentCustomField
        {
            public string DisplayName { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
        }

        public class CustomFieldGroup
        {
            public string ContainerId { get; set; }
            public List<IncidentCustomField> CustomFields { get; set; }
            public string GroupType { get; set; }
            public string PublicId { get; set; }
        }

        public class IncidentIncidentLocation
        {
            public object DataCenter { get; set; }
            public object DeviceGroup { get; set; }
            public object DeviceName { get; set; }
            public string Environment { get; set; }
            public object ServiceInstanceId { get; set; }
        }

        public class IncidentMitigationData
        {
            public string ChangedBy { get; set; }
            public DateTime Date { get; set; }
            public string Mitigation { get; set; }
        }

        public class IncidentRaisingLocation
        {
            public string DataCenter { get; set; }
            public string DeviceGroup { get; set; }
            public string DeviceName { get; set; }
            public string Environment { get; set; }
            public object ServiceInstanceId { get; set; }
        }

        public class IncidentResolutionData
        {
            public string ChangedBy { get; set; }
            public bool CreatePostmortem { get; set; }
            public DateTime Date { get; set; }
        }

        public class IncidentSource
        {
            public DateTime CreateDate { get; set; }
            public string CreatedBy { get; set; }
            public string IncidentId { get; set; }
            public DateTime ModifiedDate { get; set; }
            public string Origin { get; set; }
            public object Revision { get; set; }
            public string SourceId { get; set; }
        }


        public IncidentAcknowledgementData AcknowledgementData { get; set; }
        public object ChangeList { get; set; }
        public string ChildCount { get; set; }
        public object CommitDate { get; set; }
        public object CommunicationsManagerContactId { get; set; }
        public object Component { get; set; }
        public object CorrelationId { get; set; }
        public DateTime CreateDate { get; set; }
        public List<CustomFieldGroup> CustomFieldGroups { get; set; }
        public object CustomerName { get; set; }
        public object DiagnosticsLink { get; set; }
        public List<object> ExternalIncidents { get; set; }
        public string ExternalLinksCount { get; set; }
        public object HealthResourceId { get; set; }
        public string HitCount { get; set; }
        public string HowFixed { get; set; }
        public string Id { get; set; }
        public DateTime ImpactStartDate { get; set; }
        public List<object> ImpactedComponents { get; set; }
        public List<string> ImpactedServicesIds { get; set; }
        public List<object> ImpactedTeamsPublicIds { get; set; }
        public IncidentIncidentLocation IncidentLocation { get; set; }
        public object IncidentManagerContactId { get; set; }
        public object IncidentSubType { get; set; }
        public string IncidentType { get; set; }
        public bool IsCustomerImpacting { get; set; }
        public bool IsNoise { get; set; }
        public bool IsOutage { get; set; }
        public bool IsSecurityRisk { get; set; }
        public object Keywords { get; set; }
        public object LastCorrelationDate { get; set; }
        public IncidentMitigationData MitigationData { get; set; }
        public DateTime ModifiedDate { get; set; }
        public object MonitorId { get; set; }
        public object NewDescriptionEntry { get; set; }
        public string OriginatingTenantId { get; set; }
        public string OwningContactAlias { get; set; }
        public string OwningTeamId { get; set; }
        public string OwningTenantId { get; set; }
        public object ParentIncidentId { get; set; }
        public IncidentRaisingLocation RaisingLocation { get; set; }
        public object ReactivationData { get; set; }
        public string RelatedLinksCount { get; set; }
        public string ReproSteps { get; set; }
        public IncidentResolutionData ResolutionData { get; set; }
        public object ResponsibleTeamId { get; set; }
        public string ResponsibleTenantId { get; set; }
        public string RoutingId { get; set; }
        public int Severity { get; set; }
        public int SiloId { get; set; }
        public object SiteReliabilityContactId { get; set; }
        public IncidentSource Source { get; set; }
        public string SourceOrigin { get; set; }
        public string Status { get; set; }
        public string SubscriptionId { get; set; }
        public string Summary { get; set; }
        public object SupportTicketId { get; set; }
        public string Title { get; set; }
        public object TsgId { get; set; }
        public object TsgOutput { get; set; }
    }
}
