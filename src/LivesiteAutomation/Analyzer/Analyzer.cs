using LivesiteAutomation.Json2Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LivesiteAutomation
{
    public partial class Analyzer
    {
        enum ComputeType { IaaS, VMSS, PaaS, Unknown }
        public Guid SubscriptionId { get; private set; }
        public string ResourceGroupName { get; private set; }
        public string VMName { get; private set; }
        public DateTime StartTime { get; private set; }
        private int Id;
        public Analyzer(int Id)
        {
            this.Id = Id;
            (SubscriptionId, ResourceGroupName, VMName, StartTime) = AnalyzeICM();
            SALsA.GetInstance(Id)?.Log.Send("{0}", Utility.ObjectToJson(this, true));

            // TODO analyse ARM and REDFE in parallel
            var arm  = AnalyzeARMSubscription(SubscriptionId);
            var rdfe = AnalyzeRDFESubscription(SubscriptionId);

            (var type, var dep) = DetectVMType(arm, rdfe);

            if (dep == null)
            {
                SALsA.GetInstance(Id).Log.Send("Could not find VM: {0} in RG: {1}. This VM might have been already deleted or moved", this.VMName, this.ResourceGroupName);
                throw new Exception("VM not found");
            }
            
            switch (type)
            {
                case ComputeType.IaaS:
                    ExecuteAllActionsForIaaS((ARMDeployment)dep);
                    break;
                case ComputeType.VMSS:
                    ExecuteAllActionsForVMSS((ARMDeployment)dep);
                    break;
                case ComputeType.PaaS:
                     ExecuteAllActionsForPaaS((RDFEDeployment)dep);
                    break;
                default:
                    break;
            }
        }

        private void ExecuteAllActionsForIaaS(ARMDeployment dep)
        {
            SALsA.GetInstance(Id).Log.Send(dep);
            SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs(Id, dep), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMConsoleScreenshot(Id, dep), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, GenevaActions.GetVMModelAndInstanceView(Id, dep), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM(Id, dep), Id)
            );

        }
        private void ExecuteAllActionsForVMSS(ARMDeployment dep)
        {
            int instanceId = TryConvertInstanceNameToInstanceId(this.VMName);
            // TODO instead of using 0, take 5 random and use them
            instanceId = instanceId == -1 ? 0 : instanceId;

            SALsA.GetInstance(Id).Log.Send(dep);
            SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs(Id, dep, instanceId), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMConsoleScreenshot(Id, dep, instanceId), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, GenevaActions.GetVMModelAndInstanceView(Id, dep, instanceId), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM(Id, dep, instanceId), Id)
            );
        }

        private void ExecuteAllActionsForPaaS(RDFEDeployment dep)
        {
            var instance = dep.RoleInstances.Where(x => x.RoleInstanceName == VMName).FirstOrDefault();
            if (instance?.RoleInstanceName == null)
            {
                instance = dep.RoleInstances.Where(x => x.RoleName == TryConvertInstanceNameToVMName(this.VMName)).FirstOrDefault();
            }
            var vmInfo = new ShortRDFERoleInstance
            {
                Fabric = dep.FabricGeoId,
                DeploymentId = dep.Name,
                DeploymentName = dep.Id,
                ContainerID = instance.ID,
                NodeId = instance.VMID,
                InstanceName = instance.RoleInstanceName
            };
            SALsA.GetInstance(Id)?.Log.Send(vmInfo);

            SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetClassicVMConsoleScreenshot(Id, vmInfo), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerNodeDiagnosticsFilename, GenevaActions.GetNodeDiagnostics(Id, vmInfo), Id)
            );
        }

        private (ComputeType type, object dep) DetectVMType(ARMSubscription arm, RDFESubscription rdfe)
        {
            ARMDeployment[] ardDeps = arm.deployments.Where(x =>
                    x.Name.Contains(this.VMName) || this.VMName.Contains(x.Name)
                ).ToArray();
            string VMName = TryConvertInstanceNameToVMName(this.VMName);
            if (ardDeps.Length > 1)
            {
                ardDeps = ardDeps.Where(x => x.ResourceGroups == this.ResourceGroupName).ToArray();
            }
            ARMDeployment dep = new ARMDeployment();
            if(ardDeps.Length > 0)
            {
                dep = ardDeps.First();
            }
            else
            {
                // Probably paaS
                dep.Type = Constants.AnalyzerARMDeploymentPaaSType;
            }

            switch (dep.Type)
            {
                case Constants.AnalyzerARMDeploymentIaaSType:
                    return (ComputeType.IaaS, dep);
                case Constants.AnalyzerARMDeploymentVMSSType:
                    return (ComputeType.VMSS, dep);
                case Constants.AnalyzerARMDeploymentPaaSType:
                    return (ComputeType.PaaS, AnalyseRDFEPaaSDeployment(rdfe));
                default:
                    return (ComputeType.Unknown, null);
            }
            throw new NotSupportedException();
        }

        private RDFEDeployment AnalyseRDFEPaaSDeployment(RDFESubscription rdfe)
        {

            string VMName = TryConvertInstanceNameToVMNamePaaS(this.VMName);
            List<RDFEDeployment> rdfeDeps = new List<RDFEDeployment>();
            foreach (var deployment in rdfe.deployments)
            {
                foreach(var instance in deployment.RoleInstances)
                {
                    if (instance.RoleName.Contains(VMName) || VMName.Contains(instance.RoleName))
                    {
                        rdfeDeps.Add(deployment);
                        break;
                    }
                }
            }
            if (rdfeDeps.Count > 1)
            {
                rdfeDeps = rdfeDeps.Where(x => x.HostedServiceName == this.ResourceGroupName).ToList();
            }
            if (rdfeDeps.Count == 1)
            {
                return rdfeDeps.First();
            }
            else if (rdfeDeps.Count > 1)
            {
                // Best effort guess
                return rdfeDeps.Where(x => x.HostedServiceName == this.ResourceGroupName
                && x.RoleInstances.Where(y => y.RoleName == VMName).ToList().Count >= 1).ToList().First();
            }
            else
            {
                return null;
            }

        }

        private string TryConvertInstanceNameToVMName(string VMName)
        {
            try
            {
                var nameOnly = Regex.Match(VMName, @"_?([a-z][a-z0-9\-]+)_[0-9]+", RegexOptions.IgnoreCase).Groups[1].Value;
                if (string.IsNullOrEmpty(nameOnly))
                {
                    return VMName;
                }
                else
                {
                    return nameOnly;
                }
            }
            catch
            {
                return VMName;
            }
        }

        private int TryConvertInstanceNameToInstanceId(string VMName)
        {
            try
            {
                return Convert.ToInt32(Regex.Match(VMName, @"_?[a-z][a-z0-9\-]+_([0-9])+", RegexOptions.IgnoreCase).Groups[1].Value);
            }
            catch
            {
                return -1;
            }
        }

        private string TryConvertInstanceNameToVMNamePaaS(string VMName)
        {
            return VMName.Split("_IN_".ToCharArray())[0];
        }
    }
}
