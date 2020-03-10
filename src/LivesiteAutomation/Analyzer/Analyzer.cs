using LivesiteAutomation.Json2Class;
using LivesiteAutomation.Kusto;
using LivesiteAutomation.ManualRun;
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
        private bool IsCustomRun = false;
        public Analyzer(int Id)
        {
            this.Id = Id;
            Nullable<Guid> sub;
            (sub, ResourceGroupName, VMName, StartTime) = AnalyzeICM();
            var isHostIssue = AnalyzeHost();
            if (isHostIssue == true)
            {
                return;
            }
            SubscriptionId = (Guid)sub;
            SALsA.GetInstance(Id)?.Log.Send("{0}", Utility.ObjectToJson(this, true));
            AnalyzerInternal();
        }

        private void AnalyzerInternal(bool checkARM = true, bool checkRDFE = true)
        {
            // TODO analyse ARM and REDFE in parallel
            ARMSubscription arm = checkARM ? AnalyzeARMSubscription(SubscriptionId, this.ResourceGroupName) : null;
            RDFESubscription rdfe = checkRDFE ? AnalyzeRDFESubscription(SubscriptionId) : null;

            (var type, var dep) = DetectVMType(arm, rdfe);

            if (dep == null && IsCustomRun == false)
            {
                SALsA.GetInstance(Id).Log.Send("Could not find VM: {0} in RG: {1}. This VM might have been already deleted or moved", this.VMName, this.ResourceGroupName);
                throw new Exception("VM not found");
            }

            // TODO : Kusto fun
            //var a = new LivesiteAutomation.Kusto.GuestAgentGenericLogs(icm);
            //a.BuildAndSendRequest();

            CallInternalComputeTypes(type, dep);
        }

        private void CallInternalComputeTypes(ComputeType type, object dep)
        {
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

        public Analyzer(int Id, object manualRun)
        {
            this.IsCustomRun = true;
            this.Id = Id;
            StartTime = SALsA.GetInstance(Id).ICM.CurrentICM.CreateDate;
            SALsA.GetInstance(Id).Log.Information("Received ManualRun order type {0} with param {1} : ",
                manualRun.GetType(), Utility.ObjectToJson(manualRun));
            if (manualRun.GetType() == typeof(ManualRun_ICM))
            {
                ManualRun_ICM manualArm = (ManualRun_ICM)manualRun;

                SubscriptionId = manualArm.SubscriptionID;
                ResourceGroupName = manualArm.ResourceGroupName;
                VMName = manualArm.VMName;

                AnalyzerInternal();
            }
            else if (manualRun.GetType() == typeof(ManualRun_IID))
            {
                ManualRun_IID manualIID = (ManualRun_IID)manualRun;

                SubscriptionId = manualIID.SubscriptionID;
                ResourceGroupName = manualIID.ResourceGroupName;
                VMName = manualIID.VMName;

                ARMDeployment dep = null;
                int instanceId = -1;
                if (String.IsNullOrEmpty(manualIID.Region))
                {
                    SALsA.GetInstance(Id).Log.Information("Calling automatic ARM VMdetection. No Region parameter provided.");
                    ARMSubscription arm = AnalyzeARMSubscription(SubscriptionId, this.ResourceGroupName);
                    dep = AnalyzeARMDeployment(arm);
                    instanceId = TryConvertInstanceNameToInstanceId(this.VMName);
                }
                else
                {
                    dep = new ARMDeployment();
                    dep.Name = manualIID.VMName;
                    dep.Subscriptions = manualIID.SubscriptionID.ToString();
                    dep.ResourceGroups = manualIID.ResourceGroupName;
                    dep.Location = manualIID.Region;
                    instanceId = manualIID.Instance;
                }
                if (instanceId < 0)
                {
                    SALsA.GetInstance(Id).Log.Information("No Instance ID detected. Assuming this is a normal single IaaS VM");
                    SALsA.GetInstance(Id).TaskManager.AddTask(
                        Utility.SaveAndSendBlobTask(
                            Constants.AnalyzerInspectIaaSDiskOutputFilename,
                                GenevaActions.InspectIaaSDiskForARMVM(Id, dep), Id));
                }
                else
                {
                    SALsA.GetInstance(Id).Log.Information("No Instance ID detected. Assuming this is a normal single IaaS VM");
                    SALsA.GetInstance(Id).TaskManager.AddTask(
                        Utility.SaveAndSendBlobTask(
                            Constants.AnalyzerInspectIaaSDiskOutputFilename,
                                GenevaActions.InspectIaaSDiskForARMVM(Id, dep, instanceId), Id));
                }
            }
            else if (manualRun.GetType() == typeof(ManualRun_RDFE_Fabric))
            {
                var rdfe = (ManualRun_RDFE_Fabric)manualRun;
                var vmInfo = new ShortRDFERoleInstance { 
                    ContainerID = new Guid(rdfe.ContainerID),
                    Fabric = rdfe.FabricCluster,
                    NodeId = new Guid(rdfe.NodeId)
                };
                Utility.SaveAndSendBlobTask(Constants.AnalyzerNodeDiagnosticsFilename, GenevaActions.GetNodeDiagnosticsFilesByContainerId(Id, vmInfo), Id).Wait();
            }
            else if (manualRun.GetType() == typeof(ManualRun_RDFE_Tenant))
            {
                var rdfe = (ManualRun_RDFE_Tenant)manualRun;
                var vmInfo = new ShortRDFERoleInstance
                {
                    Fabric = rdfe.FabricCluster,
                    DeploymentId = rdfe.DeploymentID,
                    InstanceName = rdfe.RoleInstanceName
                };
                Utility.SaveAndSendBlobTask(Constants.AnalyzerNodeDiagnosticsFilename, GenevaActions.GetNodeDiagnosticsFilesByDeploymentIdorVMName(Id, vmInfo), Id).Wait();
            }
        }

        static void CallAndPostEG(int Id, string ContainerId)
        {
            try
            {
                var vmEGAnalysis = new VMEGAnalysis(Id).BuildAndSendRequest(ContainerId);
                SALsA.GetInstance(Id).Log.Send(vmEGAnalysis, htmlfy: false);

                var vma = new VMA(Id).BuildAndSendRequest(ContainerId);
                SALsA.GetInstance(Id).Log.Send(vma, htmlfy: false);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Critical("Failed to query EG");
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
        }

        private bool AnalyzeHost()
        {
            var currentICM = SALsA.GetInstance(Id).ICM;
            var title = currentICM.CurrentICM.Title;
            var isHostIssue = Regex.Match(title, @"HostGAPlugin.*Cluster.*Node.*(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}", RegexOptions.IgnoreCase).Success;
            if(isHostIssue)
            {
                var splitTitle = title.ToLowerInvariant().Replace(" :", ":").Replace(": ", ":").Replace(",", " ").Replace(".", " ").Replace("nodeid", "node").Split(' ');
                splitTitle = Array.FindAll(splitTitle, s => s.Contains(":"));
                var dict = splitTitle.ToDictionary(
                    k => k.Split(':')[0],
                    e => e.Split(':')[1]
                );
                var cluster = dict["cluster"];
                var nodeid = dict["node"];
                var creationTime = currentICM.CurrentICM.CreateDate;
                var startTime = creationTime.AddHours(-12);
                var endTime = new DateTime(Math.Min(creationTime.AddHours(+12).Ticks, DateTime.UtcNow.Ticks));
                SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerHostGAPluginFilename, 
                    GenevaActions.GetNodeDiagnosticsFiles(Id, cluster, nodeid,startTime.ToString("s"), endTime.ToString("s")), Id)
                );
            }
            return isHostIssue;

        }

        private void ExecuteAllActionsForIaaS(ARMDeployment dep)
        {
            Task<string> modelTask = null;
            SALsA.GetInstance(Id).Log.Send(dep);
            SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs(Id, dep), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMConsoleScreenshot(Id, dep), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, modelTask = GenevaActions.GetVMModelAndInstanceView(Id, dep), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM(Id, dep), Id)
            );

            var rawInfo = LogContainerId(modelTask, Id);
            if(rawInfo != null)
            { 
                var vmInfo = new ShortRDFERoleInstance
                {
                    ContainerID = new Guid(rawInfo.ContainerId),
                    Fabric = rawInfo.Cluster,
                    NodeId = new Guid(rawInfo.NodeId)
                };
                SALsA.GetInstance(Id).TaskManager.AddTask(
                    Utility.SaveAndSendBlobTask(Constants.AnalyzerNodeDiagnosticsFilename, GenevaActions.GetNodeDiagnosticsFilesByContainerId(Id, vmInfo), Id)
                );
            }
        }

        private AzureCMVMIdToContainerID.MessageLine LogContainerId(Task<string> modelTask, int Id)
        {

            if (modelTask != null)
            {
                try
                {
                    var model = Utility.JsonToObject<Dictionary<string, dynamic>>(modelTask.Result);
                    var vmid = (string)(model["VM Model"].properties.vmId);
                    var vmInfo = new AzureCMVMIdToContainerID(Id).BuildAndSendRequest(vmid);
                    SALsA.GetInstance(Id).Log.Send(vmInfo);

                    CallAndPostEG(Id, vmInfo.ContainerId);
                    return vmInfo;
                }
                catch (Exception ex)
                {
                    SALsA.GetInstance(Id)?.Log.Critical("Failed to populate ContainerId");
                    SALsA.GetInstance(Id)?.Log.Exception(ex);
                }
            }
            return null;
        }

        private void ExecuteAllActionsForVMSS(ARMDeployment dep)
        {
            int instanceId = TryConvertInstanceNameToInstanceId(this.VMName);
            // TODO instead of using 0, take 5 random and use them
            instanceId = instanceId == -1 ? 0 : instanceId;

            Task<string> modelTask = null;
            SALsA.GetInstance(Id).Log.Send(dep);
            SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerConsoleSerialOutputFilename, GenevaActions.GetVMConsoleSerialLogs(Id, dep, instanceId), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetVMConsoleScreenshot(Id, dep, instanceId), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMModelAndViewOutputFilename, modelTask = GenevaActions.GetVMModelAndInstanceView(Id, dep, instanceId), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerInspectIaaSDiskOutputFilename, GenevaActions.InspectIaaSDiskForARMVM(Id, dep, instanceId), Id)
            ); 
            var rawInfo = LogContainerId(modelTask, Id);
            if (rawInfo != null)
            {
                var vmInfo = new ShortRDFERoleInstance
                {
                    ContainerID = new Guid(rawInfo.ContainerId),
                    Fabric = rawInfo.Cluster,
                    NodeId = new Guid(rawInfo.NodeId)
                };
                SALsA.GetInstance(Id).TaskManager.AddTask(
                    Utility.SaveAndSendBlobTask(Constants.AnalyzerNodeDiagnosticsFilename, GenevaActions.GetNodeDiagnosticsFilesByContainerId(Id, vmInfo), Id)
                );
            }
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
                InstanceName = instance.RoleInstanceName
            };

            CallAndPostEG(Id, instance.ID.ToString());
            Task<string> modelTask = null;

            SALsA.GetInstance(Id).TaskManager.AddTask(
                Utility.SaveAndSendBlobTask(Constants.AnalyzerVMScreenshotOutputFilename, GenevaActions.GetClassicVMConsoleScreenshot(Id, vmInfo), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerNodeDiagnosticsFilename, GenevaActions.GetNodeDiagnosticsFilesByDeploymentIdorVMName(Id, vmInfo), Id),
                Utility.SaveAndSendBlobTask(Constants.AnalyzerContainerSettings, modelTask = GenevaActions.GetContainerSettings(Id, vmInfo), Id)

            );
            try
            {
                var model = Utility.JsonToObject<Json2Class.ContainerSettings>(modelTask.Result);
                vmInfo.NodeId = new Guid(model.NodeId);
            }
            catch (Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Critical("Failed to populate NodeId");
                SALsA.GetInstance(Id)?.Log.Exception(ex);
            }
            finally
            {
                SALsA.GetInstance(Id)?.Log.Send(vmInfo);
            }
        }

        private (ComputeType type, object dep) DetectVMType(ARMSubscription arm, RDFESubscription rdfe)
        {
            var armDep = AnalyzeARMDeployment(arm);
            if (armDep == null)
            {
                var paasDep = AnalyseRDFEDeployment(rdfe);
                if (paasDep == null)
                {
                    return (ComputeType.Unknown, null);
                }
                else 
                {
                    return (ComputeType.PaaS, paasDep);
                }
            }
            else
            {
                switch (armDep.Type)
                {
                    case Constants.AnalyzerARMDeploymentIaaSType:
                        return (ComputeType.IaaS, armDep);
                    case Constants.AnalyzerARMDeploymentVMSSType:
                        return (ComputeType.VMSS, armDep);
                    default:
                        return (ComputeType.Unknown, null);
                }
            }
        }

        private ARMDeployment AnalyzeARMDeployment(ARMSubscription arm)
        {
            ARMDeployment dep = new ARMDeployment();
            try
            {
                ARMDeployment[] armDeps = arm.deployments.Where(x =>
                        x.Name.ToLower().Contains(this.VMName.ToLower()) || this.VMName.ToLower().Contains(x.Name.ToLower())
                    ).ToArray();
                string VMName = TryConvertInstanceNameToVMName(this.VMName);
                if (armDeps.Length > 1)
                {
                    var smallArmDeps = arm.deployments.Where(x =>
                        x.Name.ToLower() == VMName.ToLower() || x.Name.ToLower() == this.VMName.ToLower()
                    ).ToArray();
                    if (smallArmDeps != null)
                    {
                        armDeps = smallArmDeps;
                    }
                }
                if (armDeps.Length > 1)
                {
                    SALsA.GetInstance(Id).Log.Error("Found more than one VM named {0} in RessourceGroup {1}, will take the first one.{2}{3}",
                        VMName, ResourceGroupName, Environment.NewLine, armDeps);
                }
                if (armDeps.Length > 0)
                {
                    dep = armDeps.First();
                }
                else
                {
                    // Probably paaS
                    dep = null;
                }
            }
            catch
            {
                // Probably paaS
                dep = null;
            }
            return dep;
        }

        private RDFEDeployment AnalyseRDFEDeployment(RDFESubscription rdfe)
        {
            if (rdfe == null) return null;
            string VMName = TryConvertInstanceNameToVMNamePaaS(this.VMName);
            List<RDFEDeployment> rdfeDeps = new List<RDFEDeployment>();
            rdfeDeps = rdfe.deployments.Where(x => x.HostedServiceName.ToLowerInvariant() == this.ResourceGroupName.ToLowerInvariant()
                                      || this.ResourceGroupName.ToLowerInvariant().Contains(x.Id.ToLowerInvariant())).ToList();
            if (rdfeDeps.Count > 1)
            {
                var tmp = rdfeDeps.Where(x => x.HostedServiceName.ToLowerInvariant() == this.ResourceGroupName.ToLowerInvariant()
                                      || this.ResourceGroupName.ToLowerInvariant().Contains(x.Id.ToLowerInvariant())).ToList();
                if(tmp.Count > 0)
                {
                    rdfeDeps = tmp;
                }
            }
            if (rdfeDeps.Count == 1)
            {
                return rdfeDeps.First();
            }
            else if (rdfeDeps.Count > 1)
            {
                // Best effort guess
                var paasVMName = VMName == this.VMName ? String.Format("{0}_IN_0", VMName) : this.VMName;
                var ret = rdfeDeps.Where(x => x.RoleInstances.Where(y => y.RoleInstanceName.ToLowerInvariant().Trim() == paasVMName.ToLowerInvariant().Trim()).ToList().Count >= 1).ToList();
                if(ret.Count() > 0)
                {
                    return ret.First();
                }
                else
                {
                    return rdfeDeps.First();
                }
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
                return Convert.ToInt32(Regex.Match(VMName, @"_?[a-z][a-z0-9\-]+_([0-9]+)", RegexOptions.IgnoreCase).Groups[1].Value);
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
