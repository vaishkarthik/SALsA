﻿using LivesiteAutomation.Json2Class;
using LivesiteAutomation.Kusto;
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
            Nullable<Guid> sub;
            (sub, ResourceGroupName, VMName, StartTime) = AnalyzeICM();
            var isHostIssue = AnalyzeHost();
            if (isHostIssue == true)
            {
                return;
            }
            SubscriptionId = (Guid)sub;
            SALsA.GetInstance(Id)?.Log.Send("{0}", Utility.ObjectToJson(this, true));
            // TODO analyse ARM and REDFE in parallel
            var arm  = AnalyzeARMSubscription(SubscriptionId, this.ResourceGroupName);
            var rdfe = AnalyzeRDFESubscription(SubscriptionId);

            (var type, var dep) = DetectVMType(arm, rdfe);

            if (dep == null)
            {
                SALsA.GetInstance(Id).Log.Send("Could not find VM: {0} in RG: {1}. This VM might have been already deleted or moved", this.VMName, this.ResourceGroupName);
                throw new Exception("VM not found");
            }

            // TODO : Kusto fun
            //var a = new LivesiteAutomation.Kusto.GuestAgentGenericLogs(icm);
            //a.BuildAndSendRequest();

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
            LogContainerId(modelTask, Id);
        }

        private void LogContainerId(Task<string> modelTask, int Id)
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
                }
                catch (Exception ex)
                {
                    SALsA.GetInstance(Id)?.Log.Critical("Failed to populate ContainerId");
                    SALsA.GetInstance(Id)?.Log.Exception(ex);
                }
            }
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
            LogContainerId(modelTask, Id);
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
            ARMDeployment dep = new ARMDeployment();
            try { 
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
                if(armDeps.Length > 1)
                {
                    SALsA.GetInstance(Id).Log.Error("Found more than one VM named {0} in RessourceGroup {1}, will take the first one.{2}{3}",
                        VMName, ResourceGroupName, Environment.NewLine, armDeps);
                }
                if(armDeps.Length > 0)
                {
                    dep = armDeps.First();
                }
                else
                {
                    // Probably paaS
                    dep.Type = Constants.AnalyzerARMDeploymentPaaSType;
                }
            }
            catch
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
                rdfeDeps = rdfeDeps.Where(x => x.HostedServiceName.ToLowerInvariant() == this.ResourceGroupName.ToLowerInvariant()).ToList();
            }
            if (rdfeDeps.Count == 1)
            {
                return rdfeDeps.First();
            }
            else if (rdfeDeps.Count > 1)
            {
                // Best effort guess
                var ret = rdfeDeps.Where(x => x.RoleInstances.Where(y => y.RoleName == this.VMName).ToList().Count >= 1).ToList();
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
