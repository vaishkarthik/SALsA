using LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static async Task<string> GetVMModelAndInstanceView(int icm, ARMDeployment deployment)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetVMModelAndInstanceView with params {0}", deployment);
            var model = await GetVMView(icm, deployment, Constants.GetVMInfoOptions[0]);
            var instanceview = await GetVMView(icm, deployment, Constants.GetVMInfoOptions[1]);

            return Utility.Beautify(Utility.JsonToObject<dynamic>(String.Format("{{\"VM Model\":{0},\"VM InstanceView\":{1}}}", model, instanceview)));
        }

        // TODO : make sovereign cloud available
        public static async Task<string> GetVMModelAndInstanceView(int icm, ARMDeployment deployment, int id)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetVMModelAndInstanceView of id:{0} with params {1}", id, deployment);
            var model = await GetVMView(icm, deployment, Constants.GetVMInfoOptionsVMSS[0], id);
            var instanceview = await GetVMView(icm, deployment, Constants.GetVMInfoOptionsVMSS[1], id);

            return Utility.Beautify(Utility.JsonToObject<dynamic>(String.Format("{{\"VM Model\":{0},\"VM InstanceView\":{1}}}", model, instanceview)));
        }

        public static Task<string> GetVMView(int icm, ARMDeployment deployment, string option)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetVMModelAndInstanceView of option:{0} with params {1}", option, deployment);
            var param = new GenevaOperations.GetVMModelAndInstanceView
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevmnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smegetvmoptionparameter = option
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetVMInfoExtensionName, Constants.GetVMInfoOperationName, actionParam).GetOperationResultOutputAsync(icm);

            return Task.Run(() => (
                    task.Result
                ));
        }
        public static Task<string> GetVMView(int icm, ARMDeployment deployment, string option, int id)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction GetVMModelAndInstanceView of id:{0}, option:{1} with params {2}", id, option, deployment);
            var param = new GenevaOperations.GetVMModelAndInstanceViewVMSS
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevirtualmachinescalesetnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smegetvmscalesetvmoptionparameter = option,
                smevirtualmachinescalesetvminstanceidparameter = id
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetVMInfoExtensionName, Constants.GetVMSSInfoOperationName, actionParam).GetOperationResultOutputAsync(icm);

            return Task.Run(() => (
                    task.Result
                ));
        }
    }
}