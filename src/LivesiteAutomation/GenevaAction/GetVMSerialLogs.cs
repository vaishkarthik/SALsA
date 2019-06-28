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
        public static Task<String> GetVMSerialLogs(ARMDeployment deployment)
        {
            var param = new GenevaOperations.GetVMSerialLogs
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevmnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMSerialLogsExtensionName, Constants.GetVMSerialLogsOperationName, actionParam).GetOperationFileOutputAsync();

            // VMSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                new StreamReader(Utility.ExtractZip(task.Result).Entries.First().Open(), System.Text.Encoding.UTF8).ReadToEnd()
                ));
        }
    }
}
