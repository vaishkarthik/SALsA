using System;

namespace SALsA.LivesiteAutomation.Json2Class
{
    public class ContainerSettings
    {
        public string ContainerId { get; set; }
        public string TenantName { get; set; }
        public string RoleInstanceId { get; set; }
        public string NodeId { get; set; }
        public string Type { get; set; }
        public string SizePolicy { get; set; }
        public string State { get; set; }
        public string OsStateHyperVHeartbeat { get; set; }
        public string IsExpectedToRun { get; set; }
        public string IsNmProgrammingComplete { get; set; }
        public string IsNetworkAllocationComplete { get; set; }
        public string IsCreated { get; set; }
        public string IsRunning { get; set; }
        public string IsLbProgrammingComplete { get; set; }
        public string ExpectedRunningStateChangedReason { get; set; }
        public DateTime ExpectedRunningStateChangedUtc { get; set; }
        public string IsFaulted { get; set; }
        public string FaultInfo { get; set; }
        public string WorkflowState { get; set; }
        public string DesiredState { get; set; }
        public string IsExplicitlyStopped { get; set; }
        public string LifecycleState { get; set; }
        public string IsEnabled { get; set; }
        public string IsConnected { get; set; }
        public string AreNetworkResourcesReleased { get; set; }
        public string IsIsolationRequested { get; set; }
        public string LogicalHostUpdateDomain { get; set; }
        public string LogicalFaultDomain { get; set; }
        public string PhysicalHostUpdateDomain { get; set; }
        public string PhysicalFaultDomain { get; set; }
        public string macAddress { get; set; }
        public object vfpFilters { get; set; }
        public Statusofendpoint[] statusofendpoints { get; set; }
        public string CommunicationIpAddress { get; set; }
        public string GuestOSName { get; set; }
        public string GuestOSVersion { get; set; }
        public string GuestOSKernelVersion { get; set; }
        public string VmHyperVIcHeartbeatData { get; set; }
        public string VmPowerStateData { get; set; }
        public string HyperVHandshakeData { get; set; }
        public string VscStateData { get; set; }
    }

    public class Statusofendpoint
    {
        public string VipEndPoint { get; set; }
        public string Protocol { get; set; }
        public string Dip { get; set; }
        public int DipPort { get; set; }
        public string EndpointName { get; set; }
        public bool InService { get; set; }
        public bool IsProgrammed { get; set; }
    }
}