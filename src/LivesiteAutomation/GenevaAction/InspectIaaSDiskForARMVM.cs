using LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<Stream> InspectIaaSDiskForARMVM(ARMDeployment deployment)
        {
            Log.Instance.Information("Calling InspectIaaSDiskForARMVM with params {0}", deployment);
            var param = new GenevaOperations.InspectIaaSDiskForARMVM
            {
                smecrpregion = deployment.location,
                smeresourcegroupname = deployment.resourceGroups,
                smevmname = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smelogextractmode = Constants.InspectIaaSDiskForARMVMMode,
                smeskiptostep = Constants.InspectIaaSDiskForARMVMStep,
                smetimeoutinmins = Constants.InspectIaaSDiskForARMVMTimeout
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.InspectIaaSDiskForARMVMExtensionName, Constants.InspectIaaSDiskForARMVMOperationName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                task.Result
                ));
        }
        // TODO : make sovereign cloud available
        public static Task<Stream> InspectIaaSDiskForARMVM(ARMDeployment deployment, int id)
        {
            Log.Instance.Information("Calling InspectIaaSDiskForARMVM of id:{0} with params {1}", id, deployment);
            var param = new GenevaOperations.InspectIaaSDiskForARMVMVMSS
            {
                smecrpregion = deployment.location,
                smeresourcegroupname = deployment.resourceGroups,
                smevmssname = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smelogextractmode = Constants.InspectIaaSDiskForARMVMMode,
                smeskiptostep = Constants.InspectIaaSDiskForARMVMStep,
                smetimeoutinmins = Constants.InspectIaaSDiskForARMVMTimeout,
                smevmssinstanceid = id
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.InspectIaaSDiskForARMVMExtensionName, Constants.InspectIaaSDiskForARMVMSSOperationName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                task.Result
                ));
        }
    }
}
