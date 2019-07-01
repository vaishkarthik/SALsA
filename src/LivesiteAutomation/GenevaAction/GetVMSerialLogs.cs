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
        public static Task<String> GetVMConsoleSerialLogs(ARMDeployment deployment)
        {
            var param = new GenevaOperations.GetVMConsoleSerialLogs
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevmnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMConsoleSerialLogsExtensionName, Constants.GetVMConsoleSerialLogsOperationName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                new StreamReader(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open(), System.Text.Encoding.UTF8).ReadToEnd()
                ));
        }
        // TODO : make sovereign cloud available
        public static Task<String> GetVMConsoleSerialLogs(ARMDeployment deployment, int id)
        {
            var param = new GenevaOperations.GetVMConsoleSerialLogsVMSS
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevirtualmachinescalesetnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smevirtualmachinescalesetvminstanceidparameter = id
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMConsoleSerialLogsExtensionName, Constants.GetVMSSConsoleSerialLogsOperationName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                new StreamReader(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open(), System.Text.Encoding.UTF8).ReadToEnd()
                ));
        }
    }
}
