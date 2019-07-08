using LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class Analyzer
    {
        ARMSubscription AnalyzeARMSubscription(Guid subscriptionId)
        {
            try
            { 
                var arm = GenevaActions.GetARMSubscription(Id, subscriptionId).Result;
                var armSubscription = AnalyzeARMSubscriptionResult(arm);
                
                return armSubscription;
            }
            catch(Exception ex)
            {
                SALsA.GetInstance(Id)?.Log.Error("Unable to get or analyse the ARM subscription {0}", this.SubscriptionId);
                SALsA.GetInstance(Id)?.Log.Exception(ex);
                return null;
            }
        } 

        private ARMSubscription AnalyzeARMSubscriptionResult(string json)
        {
            var armSubscription = Utility.JsonToObject<ARMSubscriptionRaw>(json);
            var armAnalysed = new Dictionary<int, ARMDeployment>();
            foreach (var deployment in armSubscription.value)
            {
                //"id": "/subscriptions/{sub}/resourceGroups/{rg}/providers/{provider}/{type}/{name}",
                var id = deployment.id.Split('/');
                if (!Constants.AnalyszerARMDeploymentTypes.Contains(deployment.type.Split('/')[1]))
                {
                    continue;
                }
                var dep = new ARMDeployment
                {
                    subscriptions = SubscriptionId.ToString(),
                    resourceGroups = id[4],
                    location = Constants.CRPRegions.Where(x => String.Equals(x, deployment.location, StringComparison.OrdinalIgnoreCase)).FirstOrDefault(),
                    name = deployment.name.Contains("/") ? deployment.name.Split('/')[1] : deployment.name,
                    type = deployment.type.Split('/')[1]
                };
                if (!armAnalysed.ContainsKey(dep.GetHashCode()))
                {
                    armAnalysed[dep.GetHashCode()] = dep;
                }
                if (deployment.type.Split('/').Last() == Constants.AnalyzerARMDeploymentExtensionType)
                {
                    armAnalysed[dep.GetHashCode()].extensions.Add(id.Last()); ;
                }
            }
            var deployments = new ARMSubscription() { deployments = armAnalysed.Values.Cast<ARMDeployment>().ToList() };
            return deployments;
        }
    }
}
