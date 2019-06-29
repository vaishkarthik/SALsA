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
                    Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs((ARMDeployment)dep));
                    Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMScreenshot((ARMDeployment)dep));
                    //Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, GenevaActions.GetVMModelAndInstanceView((ARMDeployment)dep));
                    //Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM((ARMDeployment)dep));
                    break;
                case ComputeType.VMSS:
                    // TODO
                    break;
                case ComputeType.PaaS:
                    break;
                default:
                    break;
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
                    break;
                case Constants.AnalyzerARMDeploymentVMSSType:
                    return (ComputeType.VMSS, dep);
                    break;
                case Constants.AnalyzerARMDeploymentPaaSType:
                    // TODO
                    //return (ComputeType.PaaS, paasDeps.First());
                    break;
                default:
                    return (ComputeType.Unknown, null);
                    break;
            }
            throw new NotSupportedException();
        }
    }
}
