using LivesiteAutomation.Json2Class;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            ARMDeployment[] deps = arm.deployments.Where(x =>
                    x.name.Contains(this.VMName) || this.VMName.Contains(x.name)
                ).ToArray();
            if (deps.Length > 1)
            {
                deps = deps.Where(x => x.resourceGroups == this.ResourceGroupName).ToArray();
            }
            ARMDeployment dep = new ARMDeployment();
            if(deps.Length > 0)
            {
                dep = deps.First();
            }
            else
            {
                deps = deps.Where(x => x.resourceGroups == this.ResourceGroupName).ToArray();
                if (deps.Length > 0)
                {
                    dep = deps.First();
                }
                else
                {
                    // Probably paaS
                    dep.type = Constants.AnalyzerARMDeploymentPaaSType;
                }
            }

            switch (dep.type)
            {
                case Constants.AnalyzerARMDeploymentIaaSType:
                    return (ComputeType.IaaS, dep);
                case Constants.AnalyzerARMDeploymentVMSSType:
                    return (ComputeType.VMSS, dep);
                case Constants.AnalyzerARMDeploymentPaaSType:
                    // TODO
                    //return (ComputeType.PaaS, paasDeps.First());
                default:
                    return (ComputeType.Unknown, null);
            }
            throw new NotSupportedException();
        }
    }
}
