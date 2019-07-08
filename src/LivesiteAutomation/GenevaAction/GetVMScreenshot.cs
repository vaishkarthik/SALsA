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
        public static Task<Image> GetVMConsoleScreenshot(ARMDeployment deployment)
        {
            Log.Instance.Information("Calling GenevaAction with params {0}", deployment);
            var param = new GenevaOperations.GetVMConsoleScreenshot
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevmnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMConsoleScreenshotExtensionName, Constants.GetVMConsoleScreenshotOperationName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    Image.FromStream(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open())
                )) ;
        }
        // TODO : make sovereign cloud available
        public static Task<Image> GetVMConsoleScreenshot(ARMDeployment deployment, int id)
        {
            Log.Instance.Information("Calling GenevaAction of id:{0} with params {1}", id, deployment);
            var param = new GenevaOperations.GetVMConsoleScreenshotVMSS
            {
                smecrpregion = deployment.location,
                smeresourcegroupnameparameter = deployment.resourceGroups,
                smevirtualmachinescalesetnameparameter = deployment.name,
                wellknownsubscriptionid = deployment.subscriptions,
                smevirtualmachinescalesetvminstanceidparameter = id

            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetVMConsoleScreenshotExtensionName, Constants.GetVMSSConsoleScreenshotOperationName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    Image.FromStream(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open())
                ));
        }
    }
}
