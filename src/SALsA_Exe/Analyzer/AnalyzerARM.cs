﻿using SALsA.LivesiteAutomation.Json2Class;
using SALsA.LivesiteAutomation.Kusto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SALsA.General;

namespace SALsA.LivesiteAutomation
{
    public partial class Analyzer
    {
        ARMSubscription AnalyzeARMSubscription(Guid subscriptionId, string ressourceGroupName)
        {
            try
            {
                var arm = GenevaActions.GetARMSubscriptionRG(Id, subscriptionId, ressourceGroupName).Result;
                var armSubscription = AnalyzeARMSubscriptionResult(arm);

                return armSubscription;
            }
            catch (Exception ex)
            {
                Log.Error("Unable to get or analyse the ARM subscription {0}", this.SubscriptionId);
                Log.Exception(ex);
                return null;
            }
        }

        private ARMSubscription AnalyzeARMSubscriptionResult(string json)
        {
            var armSubscription = Utility.JsonToObject<ARMSubscriptionRaw>(json);
            if (armSubscription.value == null) return null;
            var armAnalysed = new Dictionary<string, ARMDeployment>();
            foreach (var deployment in armSubscription.value)
            {
                try
                {
                    //"id": "/subscriptions/{sub}/resourceGroups/{rg}/providers/{provider}/{type}/{name}",
                    var id = deployment.id.Split('/');
                    if (!Constants.AnalyszerARMDeploymentTypes.Contains(deployment.type.Split('/')[1]))
                    {
                        continue;
                    }
                    var dep = new ARMDeployment
                    {
                        Subscriptions = SubscriptionId.ToString(),
                        ResourceGroups = id[4],
                        Location = Constants.CRPRegions.Where(x => String.Equals(x, deployment.location, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(),
                        Name = deployment.name.Contains("/") ? deployment.name.Split('/')[0] : deployment.name,
                        Type = deployment.type.Split('/')[1]
                    };
                    if (String.IsNullOrEmpty(dep.Location))
                    {
                        dep.Location = deployment.location;
                    }
                    if (!armAnalysed.ContainsKey(dep.Name))
                    {
                        armAnalysed[dep.Name] = dep;
                    }
                    if (deployment.type.Split('/').Last() == Constants.AnalyzerARMDeploymentExtensionType)
                    {
                        armAnalysed[dep.Name].Extensions.Add(id.Last()); ;
                    }
                }
                catch (Exception)
                {
                    Log.Warning("Unable to get or analyse the ARM deployment {0}", deployment);
                    continue;
                }
            }
            var deployments = new ARMSubscription() { deployments = armAnalysed.Values.Cast<ARMDeployment>().ToList() };
            return deployments;
        }


        public VMA2ContainerId.MessageLine[] AnalyzeARMResourceURI(string subscriptions, string resourceGroups, string virtualMachines)
        {
            var vminfo = new VMA2ContainerId(Id, virtualMachines, subscriptions, resourceGroups);

            if (vminfo.Results.Length == 0)
            {
                throw new Exception(String.Format(
                    "Kusto query for Deployment {0}//{1}//{2} returned empty results", subscriptions, resourceGroups, virtualMachines));
            }

            SALsA.GetInstance(Id)?.ICM.QueueICMDiscussion(vminfo.HTMLResults, htmlfy: false);
            Log.Information(vminfo.Results);
            return vminfo.Results;
        }
    }
}
