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
        public static Task<String> GetVMConsoleSerialLogs(int icm, ARMDeployment deployment)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GetVMConsoleSerialLogs with params {0}", deployment);
            var param = new GenevaOperations.GetVMConsoleSerialLogs
            {
                smecrpregion = deployment.Location,
                smeresourcegroupnameparameter = deployment.ResourceGroups,
                smevmnameparameter = deployment.Name,
                wellknownsubscriptionid = deployment.Subscriptions
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetVMConsoleSerialLogsExtensionName, Constants.GetVMConsoleSerialLogsOperationName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                new StreamReader(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open(), System.Text.Encoding.UTF8).ReadToEnd()
                ));
        }
        // TODO : make sovereign cloud available
        public static Task<String> GetVMConsoleSerialLogs(int icm, ARMDeployment deployment, int id)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GetVMConsoleSerialLogs of id:{0} with params {1}", id, deployment);
            var param = new GenevaOperations.GetVMConsoleSerialLogsVMSS
            {
                smecrpregion = deployment.Location,
                smeresourcegroupnameparameter = deployment.ResourceGroups,
                smevirtualmachinescalesetnameparameter = deployment.Name,
                wellknownsubscriptionid = deployment.Subscriptions,
                smevirtualmachinescalesetvminstanceidparameter = id
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetVMConsoleSerialLogsExtensionName, Constants.GetVMSSConsoleSerialLogsOperationName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                new StreamReader(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open(), System.Text.Encoding.UTF8).ReadToEnd()
                ));
        }
    }
}
