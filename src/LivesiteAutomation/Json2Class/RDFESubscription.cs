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
        public string RoleInstanceName { get; set; }
        public string RoleState { get; set; }
        public string RoleStateDetails { get; set; }
        public string InstanceStatus { get; set; }
        public string InstanceErrorCode { get; set; }
        public string HostName { get; set; }
        public IPAddress IpAddress { get; set; }
        public string PowerState { get; set; }
        public bool IsAlerted { get; set; }
        public string BillingContext { get; set; }
        public int UpdateDomain { get; set; }
        public string LastRoleStartResult { get; set; }
        public DateTime LastRoleStartUtcTime { get; set; }
        public DateTime LastRoleAbortUtcTime { get; set; }
        public int CountOfRoleAborts { get; set; }
        public int CountOfRoleAbortsInTimeWindow { get; set; }
        public int CountOfStartRoleFailures { get; set; }
        public int CountOfSuccessfulStartRoles { get; set; }
        public int LastStartRoleFailureErrorCode { get; set; }
        public DateTime LastStartRoleFailureUtcTime { get; set; }
        public DateTime LastSuccessfulStartRoleUtcTime { get; set; }
        public int LogicalFaultDomain { get; set; }
        public int PhysicalFaultDomain { get; set; }
        public Guid VMID { get; set; }
        public Guid ID { get; set; }
        public string Size { get; set; }
        public int NumProcessors { get; set; }
        public int MemoryInMBs { get; set; }
        public string OS { get; set; }
        public string VirtualMachineWorkflowStep { get; set; }
        public string LastVMStartResult { get; set; }
        public int LastVMStartFailureErrorCode { get; set; }
        public string LastVMStartFailureFabricOperation { get; set; }
        public DateTime LastVMStartFailureTime { get; set; }
    }

    class RDFEDeployment
    {
        public DateTime LastRefreshTime { get; set; }
        public string Label { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string DnsName { get; set; }
        public string HostedServiceName { get; set; }
        public string VIP { get; set; }
        public string ProgressStatus { get; set; }
        public string Status { get; set; }
        public string DetailLevel { get; set; }
        public string Settings { get; set; }
        public DateTime LastChangingOperationStarted { get; set; }
        public string Locked { get; set; }
        public string RollbackAllowed { get; set; }
        public string HasPackageSaved { get; set; }
        public string VirtualNetworkName { get; set; }
        public string VirtualNetworkId { get; set; }
        public string Created { get; set; }
        public string LastModified { get; set; }
        public string Kind { get; set; }
        public string SdkVersion { get; set; }
        public string NumUpgradeDomains { get; set; }
        public string CurrentUpgradeDomain { get; set; }
        public string CurrentUpgradeDomainState { get; set; }
        public string RoleToUpgrade { get; set; }
        public string DeploymentTransitionStatus { get; set; }
        public string VirtualIPs { get; set; }
        public string VipIPv6 { get; set; }
        public string CurrentGuestAgentRelease { get; set; }
        public string CurrentGuestAgentFamily { get; set; }
        public string PinnedGuestAgentFamily { get; set; }
        public string PinnedGuestAgentRelease { get; set; }
        public List<RDFERoleInstance> RoleInstances {get; set;}
}

    class RDFESubscription
    {
        public List<RDFEDeployment> deployments { get; set; } = new List<RDFEDeployment>();
    }
}
