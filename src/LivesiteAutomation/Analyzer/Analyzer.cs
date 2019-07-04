using LivesiteAutomation.Json2Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public Task task { get; private set; }
        public Analyzer(ref ICM icm)
        {
            (SubscriptionId, ResourceGroupName, VMName, StartTime) = AnalyzeICM(icm);
            Log.Instance.Send("{0}", Utility.ObjectToJson(this, true));

            // TODO analyse ARM and REDFE in parallel
            var arm  = AnalyzeARMSubscription(SubscriptionId);
            var rdfe = AnalyzeRDFESubscription(SubscriptionId);

            (var type, var dep) = DetectVMType(arm, rdfe);
            switch (type)
            {
                case ComputeType.IaaS:
                    task = ExecuteAllActionsForIaaS((ARMDeployment)dep);
                    break;
                case ComputeType.VMSS:
                    task = ExecuteAllActionsForVMSS((ARMDeployment)dep);
                    break;
                case ComputeType.PaaS:
                    task = ExecuteAllActionsForPaaS((RDFEDeployment)dep);
                    break;
                default:
                    break;
            }
        }

        private async Task ExecuteAllActionsForIaaS(ARMDeployment dep)
        {
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs(dep));
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMConsoleScreenshot(dep));
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, GenevaActions.GetVMModelAndInstanceView(dep));
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM(dep));

        }
        private async Task ExecuteAllActionsForVMSS(ARMDeployment dep)
        {
            int instanceId = TryConvertInstanceNameToInstanceId(this.VMName);
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs(dep, instanceId));
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMConsoleScreenshot(dep, instanceId));
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, GenevaActions.GetVMModelAndInstanceView(dep, instanceId));
            await Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM(dep, instanceId));
        }

        private async Task ExecuteAllActionsForPaaS(RDFEDeployment dep)
        {

        }

        public void Wait()
        {
            try
            {
                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Log.Instance.Error("Failed to wait for internal task");
                Log.Instance.Exception(ex);
            }
        }

        private (ComputeType type, object dep) DetectVMType(ARMSubscription arm, RDFESubscription rdfe)
        {
            ARMDeployment[] ardDeps = arm.deployments.Where(x =>
                    x.name.Contains(this.VMName) || this.VMName.Contains(x.name)
                ).ToArray();
            string VMName = TryConvertInstanceNameToVMName(this.VMName);
            if (ardDeps.Length > 1)
            {
                ardDeps = ardDeps.Where(x => x.resourceGroups == this.ResourceGroupName).ToArray();
            }
            ARMDeployment dep = new ARMDeployment();
            if(ardDeps.Length > 0)
            {
                dep = ardDeps.First();
            }
            else
            {
                ardDeps = ardDeps.Where(x => x.resourceGroups == this.ResourceGroupName).ToArray();
                if (ardDeps.Length > 0)
                {
                    dep = ardDeps.First();
                }
                else
                {
                    // Probably paaS
                    ((ARMDeployment)dep).type = Constants.AnalyzerARMDeploymentPaaSType;
                }
            }

            switch (dep.type)
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
            return null;
            /*
            RDFEDeployment[] rdfeDeps = rdfe.deployments.Where(x =>
                    x.HostedServiceName.Contains(this.ResourceGroupName) || this.ResourceGroupName.Contains(x.HostedServiceName)
                ).ToArray();



            string VMName = TryConvertInstanceNameToVMName(this.VMName);
            if (ardDeps.Length > 1)
            {
                ardDeps = ardDeps.Where(x => x.resourceGroups == this.ResourceGroupName).ToArray();
            }
            object dep = new ARMDeployment();
            if (ardDeps.Length > 0)
            {
                dep = ardDeps.First();
            }
            else
            {
                ardDeps = ardDeps.Where(x => x.resourceGroups == this.ResourceGroupName).ToArray();
                if (ardDeps.Length > 0)
                {
                    dep = ardDeps.First();
                }
                else
                {
                    // Probably paaS
                    ((ARMDeployment)dep).type = Constants.AnalyzerARMDeploymentPaaSType;
                }
            }
            */
        }

        private string TryConvertInstanceNameToVMName(string VMName)
        {
            try
            {
                return Regex.Match(this.VMName, @"_?([a-z][a-z0-9\-]+)_[0-9]+", RegexOptions.IgnoreCase).Groups[1].Value;
            }
            catch
            {
                return VMName;
            }
        }

        private int TryConvertInstanceNameToInstanceId(string vMName)
        {
            try
            {
                return Convert.ToInt32(Regex.Match(this.VMName, @"_?[a-z][a-z0-9\-]+_([0-9])+", RegexOptions.IgnoreCase).Groups[1].Value);
            }
            catch
            {
                return 0;
            }
        }
    }
}
