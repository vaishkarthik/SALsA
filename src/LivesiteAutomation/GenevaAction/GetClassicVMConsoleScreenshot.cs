using LivesiteAutomation.Json2Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LivesiteAutomation
{
    public partial class GenevaActions
    {
        // TODO : make sovereign cloud available
        public static Task<Image> GetClassicVMConsoleScreenshot(ShortRDFERoleInstance instance)
        {
            Log.Instance.Information("Calling GenevaAction GetARMSubscription with params {0}", instance);
            var param = new GenevaOperations.GetClassicVMScreenshot {
                smefabrichostparam = instance.Fabric,
                smescreenshotsizeparam = Constants.GetClassicVMClassicScreenshotSize,
                smetenantnameparam = instance.DeploymentName,
                smevmnameparam = instance.InstanceName
            };
            var actionParam = Utility.JsonToObject<Dictionary<string, string>>(Utility.ObjectToJson(param));
            var task = new GenevaAction(Constants.GetClassicVMClassicScreenshotExtensionName, Constants.GetClassicVMClassicScreenshotOperatorName, actionParam).GetOperationFileOutputAsync();

            // VMConsoleSerialLog contain only one file, compressed in a zip.
            return Task.Run(() => (
                    Image.FromStream(Utility.ExtractZip(task.Result).Entries.Where(x => x.Name != "").First().Open())
                ));
        }
    }
}
