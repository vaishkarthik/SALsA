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
        public static async Task<string> GetVMModelAndInstanceView(ARMDeployment deployment)
        {
            var model = await GetVMView(deployment, Constants.GetVMInfoOptions[0]);
            var instanceview = await GetVMView(deployment, Constants.GetVMInfoOptions[1]);

            return Utility.Beautify(Utility.JsonToObject<dynamic>(String.Format("{{\"VM Model\":{0},\"VM InstanceView\":{1}}}", model, instanceview)));
        }

        public static Task<string> GetVMView(ARMDeployment deployment, string option)
        {
            var param = new GenevaOperations.GetVMModelAndInstanceView
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevmnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smegetvmoptionparameter = option
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMInfoExtensionName, Constants.GetVMInfoOperationName, actionParam).GetOperationResultOutputAsync();

            return Task.Run(() => (
                    task.Result
                ));
        }
    }
}