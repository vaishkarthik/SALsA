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
        public static Task<Image> GetVMConsoleScreenshot(int icm, ARMDeployment deployment)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction with params {0}", deployment);
            var param = new GenevaOperations.GetVMConsoleScreenshot
            {
                smecrpregion = deployment.Location,
                smeresourcegroupnameparameter = deployment.ResourceGroups,
                smevmnameparameter = deployment.Name,
                wellknownsubscriptionid = deployment.Subscriptions
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetVMConsoleScreenshotExtensionName, Constants.GetVMConsoleScreenshotOperationName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    Image.FromStream(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open())
                )) ;
        }
        // TODO : make sovereign cloud available
        public static Task<Image> GetVMConsoleScreenshot(int icm, ARMDeployment deployment, int id)
        {
            SALsA.GetInstance(icm)?.Log.Information("Calling GenevaAction of id:{0} with params {1}", id, deployment);
            var param = new GenevaOperations.GetVMConsoleScreenshotVMSS
            {
                smecrpregion = deployment.Location,
                smeresourcegroupnameparameter = deployment.ResourceGroups,
                smevirtualmachinescalesetnameparameter = deployment.Name,
                wellknownsubscriptionid = deployment.Subscriptions,
                smevirtualmachinescalesetvminstanceidparameter = id

            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(icm, Constants.GetVMConsoleScreenshotExtensionName, Constants.GetVMSSConsoleScreenshotOperationName, actionParam).GetOperationFileOutputAsync(icm);

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    Image.FromStream(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open())
                ));
        }
    }
}
