using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation.Json2Class
{
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
    }

    class RDFESubscription
    {
        public List<RDFEDeployment> deployments { get; set; } = new List<RDFEDeployment>();
    }
}
