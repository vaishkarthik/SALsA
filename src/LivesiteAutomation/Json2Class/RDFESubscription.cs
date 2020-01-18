using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Json2Class
{

    class RDFERoleInstance
    {
        public string ApplicationPackage { get; set; }
        public string BillingContext { get; set; }
        public Nullable<long> CountOfRoleAborts { get; set; }
        public Nullable<long> CountOfRoleAbortsInTimeWindow { get; set; }
        public Nullable<long> CountOfStartRoleFailures { get; set; }
        public Nullable<long> CountOfSuccessfulStartRoles { get; set; }
        public Nullable<long> EndpointPort { get; set; }
        public string EndpointVIP { get; set; }
        public string HostName { get; set; }
        public Guid ID { get; set; }
        public Nullable<long> InstanceCount { get; set; }
        public Nullable<long> InstanceErrorCode { get; set; }
        public string InstanceStatus { get; set; }
        public string IpAddress { get; set; }
        public string IsAlerted { get; set; }
        public DateTime LastRoleAbortUtcTime { get; set; }
        public string LastRoleStartResult { get; set; }
        public DateTime LastRoleStartUtcTime { get; set; }
        public Nullable<long> LastStartRoleFailureErrorCode { get; set; }
        public DateTime LastStartRoleFailureUtcTime { get; set; }
        public DateTime LastSuccessfulStartRoleUtcTime { get; set; }
        public Nullable<long> LastVMStartFailureErrorCode { get; set; }
        public string LastVMStartFailureFabricOperation { get; set; }
        public DateTime LastVMStartFailureTime { get; set; }
        public string LastVMStartResult { get; set; }
        public Nullable<long> LogicalFaultDomain { get; set; }
        public string ManagementRole { get; set; }
        public Nullable<long> MemoryInMBs { get; set; }
        public Nullable<long> NumProcessors { get; set; }
        public string OS { get; set; }
        public string OsVersion { get; set; }
        public Nullable<long> PhysicalFaultDomain { get; set; }
        public string PowerState { get; set; }
        public string RoleInstanceName { get; set; }
        public string RoleName { get; set; }
        public string RoleState { get; set; }
        public string RoleStateDetails { get; set; }
        public string Size { get; set; }
        public Nullable<long> UpdateDomain { get; set; }
        public Guid VMID { get; set; }
        public string VirtualMachineWorkflowStep { get; set; }
    }

    class RDFEDeployment
    {
        public string AccessRestrictionLevel { get; set; }
        public string AllowLazyGeoAllocation { get; set; }
        public DateTime Created { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CurrentGuestAgentFamily { get; set; }
        public string CurrentGuestAgentRelease { get; set; }
        public string CurrentUpdateDomain { get; set; }
        public string CurrentUpgradeDomain { get; set; }
        public string CurrentUpgradeDomainState { get; set; }
        public string CurrentWeightedComputeExtraSmallRoleInstances { get; set; }
        public string CurrentWeightedComputeRoleInstances { get; set; }
        public string DeploymentTransitionStatus { get; set; }
        public string Description { get; set; }
        public string DetailLevel { get; set; }
        public string DnsName { get; set; }
        public string FabricGeoId { get; set; }
        public string FabricTenantStatus { get; set; }
        public string GeoConstraint { get; set; }
        public string GeoId { get; set; }
        public string GuestAgentType { get; set; }
        public string HasPackageSaved { get; set; }
        public string HostedServiceCPXId { get; set; }
        public string HostedServiceDescription { get; set; }
        public string HostedServiceLabel { get; set; }
        public string HostedServiceName { get; set; }
        public string HttpStatusCode { get; set; }
        public string Id { get; set; }
        public string IsComplete { get; set; }
        public string Kind { get; set; }
        public string Label { get; set; }
        public DateTime LastChangingOperationStarted { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime LastRefreshTime { get; set; }
        public DateTime LastServiceHealingTime { get; set; }
        public string Locked { get; set; }
        public string Name { get; set; }
        public string NumUpgradeDomains { get; set; }
        public string OperationId { get; set; }
        public string OperationKind { get; set; }
        public string OperationStatus { get; set; }
        public string PinnedGuestAgentFamily { get; set; }
        public string PinnedGuestAgentRelease { get; set; }
        public string ProgressStatus { get; set; }
        public string ResourceGroup { get; set; }
        public string ResourceLocation { get; set; }
        public string RoleToUpgrade { get; set; }
        public string RollbackAllowed { get; set; }
        public string SdkVersion { get; set; }
        public string ServiceHealingEnabled { get; set; }
        public string ServiceInstanceCount { get; set; }
        public string Settings { get; set; }
        public string Status { get; set; }
        public DateTime TimeCompleted { get; set; }
        public DateTime TimeStarted { get; set; }
        public string UpdateInProgress { get; set; }
        public string UpdateType { get; set; }
        public string UserSelectedLocation { get; set; }
        public string VIP { get; set; }
        public string VipIPv6 { get; set; }
        public string VirtualIPs { get; set; }
        public string VirtualNetworkId { get; set; }
        public string VirtualNetworkName { get; set; }
        public List<RDFERoleInstance> RoleInstances {get; set;}
}

    class RDFESubscription
    {
        public List<RDFEDeployment> deployments { get; set; } = new List<RDFEDeployment>();
    }

    public class ShortRDFERoleInstance
    {
        public string InstanceName;
        public string DeploymentId;
        public string DeploymentName;
        public string Fabric;
        public Guid ContainerID;
        public Guid NodeId;
        public override string ToString(){return Utility.ObjectToJson(this, true);}
    }
}
