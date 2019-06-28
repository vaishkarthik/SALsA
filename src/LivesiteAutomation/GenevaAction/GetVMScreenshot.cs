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
        public static Task<Stream> GetVMConsoleScreenshot(ARMDeployment deployment)
        {
            var param = new GenevaOperations.GetVMConsoleScreenshot
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevmnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMScreenshotExtensionName, Constants.GetVMScreenshotOperationName, actionParam).GetOperationFileOutputAsync();

            // VMSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                Utility.ExtractZip(task.Result).Entries.First().Open()
                )) ;
        }
    }
}